using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HabitTrackerApp.Maui.Models;

namespace HabitTrackerApp.Maui.Services
{
    public enum SyncState
    {
        Idle,
        Syncing,
        SyncFailed,
        SyncComplete
    }

    public enum ChangeType
    {
        Create,
        Update,
        Delete
    }

    public interface ISyncService
    {
        SyncState CurrentState { get; }
        DateTime LastSyncTime { get; }
        event EventHandler<SyncState> SyncStateChanged;
        
        Task<bool> SyncAsync(CancellationToken cancellationToken = default);
        Task QueueChangeAsync<T>(T item, ChangeType changeType) where T : IHasId;
        Task<int> GetPendingChangesCountAsync();
        Task ClearPendingChangesAsync();
        Task StartPeriodicSyncAsync(TimeSpan interval, CancellationToken cancellationToken = default);
        void StopPeriodicSync();
    }

    // Represents a pending change that needs to be synced to the server
    public class PendingChange
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int Id { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string EntityJson { get; set; }
        public ChangeType ChangeType { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SyncAttempts { get; set; }
        public DateTime? LastAttemptTime { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class SyncService : ISyncService
    {
        private readonly ILogger<SyncService> _logger;
        private readonly IDatabaseService _databaseService;
        private readonly IConnectivityService _connectivityService;
        private readonly IErrorHandlingService _errorHandlingService;
        private System.Timers.Timer _syncTimer;
        private SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);
        private SyncState _currentState = SyncState.Idle;
        private DateTime _lastSyncTime;
        private bool _isInitialized = false;

        public SyncState CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    SyncStateChanged?.Invoke(this, _currentState);
                }
            }
        }

        public DateTime LastSyncTime => _lastSyncTime;

        public event EventHandler<SyncState> SyncStateChanged;

        public SyncService(
            ILogger<SyncService> logger,
            IDatabaseService databaseService,
            IConnectivityService connectivityService,
            IErrorHandlingService errorHandlingService)
        {
            _logger = logger;
            _databaseService = databaseService;
            _connectivityService = connectivityService;
            _errorHandlingService = errorHandlingService;
            
            _connectivityService.ConnectivityChanged += OnConnectivityChanged;
            
            InitializeAsync().ConfigureAwait(false);
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            try
            {
                await _databaseService.InitializeAsync();
                
                // Create PendingChange table if it doesn't exist
                var connection = await GetConnectionAsync();
                await connection.CreateTableAsync<PendingChange>();
                
                // Load last sync time from preferences
                string lastSyncStr = Preferences.Default.Get("last_sync_time", string.Empty);
                if (!string.IsNullOrEmpty(lastSyncStr) && DateTime.TryParse(lastSyncStr, out DateTime lastSync))
                {
                    _lastSyncTime = lastSync;
                }
                else
                {
                    _lastSyncTime = DateTime.MinValue;
                }
                
                _isInitialized = true;
                _logger.LogInformation("Sync service initialized. Last sync: {LastSync}", _lastSyncTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing sync service");
            }
        }

        private async Task<SQLite.SQLiteAsyncConnection> GetConnectionAsync()
        {
            // This would be a direct connection to the SQLite database
            // In a real implementation, this would be encapsulated in the DatabaseService
            // For this example, we'll simulate it by returning null
            return null;
        }

        public async Task<bool> SyncAsync(CancellationToken cancellationToken = default)
        {
            // Ensure only one sync operation runs at a time
            if (!await _syncLock.WaitAsync(0))
            {
                _logger.LogInformation("Sync already in progress, skipping request");
                return false;
            }

            try
            {
                // Update sync state
                CurrentState = SyncState.Syncing;
                
                // Check connectivity first
                if (!_connectivityService.IsConnected)
                {
                    _logger.LogInformation("Cannot sync: Device is offline");
                    CurrentState = SyncState.SyncFailed;
                    return false;
                }

                // Get all pending changes
                var pendingChanges = await GetPendingChangesAsync();
                _logger.LogInformation("Starting sync with {Count} pending changes", pendingChanges.Count);
                
                if (pendingChanges.Count == 0)
                {
                    // Even if there are no local changes, we still want to pull server changes
                    await PullServerChangesAsync(cancellationToken);
                    CurrentState = SyncState.SyncComplete;
                    UpdateLastSyncTime();
                    return true;
                }
                
                // Process each pending change
                bool allSuccessful = true;
                foreach (var change in pendingChanges)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Sync operation cancelled");
                        CurrentState = SyncState.SyncFailed;
                        return false;
                    }
                    
                    bool success = await ProcessChangeAsync(change);
                    if (!success)
                    {
                        allSuccessful = false;
                        
                        // Update the change record with failure info
                        change.SyncAttempts++;
                        change.LastAttemptTime = DateTime.UtcNow;
                        change.ErrorMessage = "Failed to sync with server";
                        await UpdatePendingChangeAsync(change);
                    }
                    else
                    {
                        // Remove the change since it's been processed
                        await DeletePendingChangeAsync(change.Id);
                    }
                }
                
                // After pushing all local changes, pull server changes
                await PullServerChangesAsync(cancellationToken);
                
                // Update state and last sync time
                CurrentState = allSuccessful ? SyncState.SyncComplete : SyncState.SyncFailed;
                if (allSuccessful)
                {
                    UpdateLastSyncTime();
                }
                
                return allSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during sync operation");
                CurrentState = SyncState.SyncFailed;
                await _errorHandlingService.HandleExceptionAsync(ex, "Sync");
                return false;
            }
            finally
            {
                _syncLock.Release();
            }
        }

        private void UpdateLastSyncTime()
        {
            _lastSyncTime = DateTime.UtcNow;
            Preferences.Default.Set("last_sync_time", _lastSyncTime.ToString("o"));
            _logger.LogInformation("Last sync time updated to: {LastSync}", _lastSyncTime);
        }

        private async Task<List<PendingChange>> GetPendingChangesAsync()
        {
            // In a real implementation, query the PendingChange table
            // For this example, return an empty list
            return new List<PendingChange>();
        }

        private async Task<bool> ProcessChangeAsync(PendingChange change)
        {
            // In a real implementation, this would send the change to the server
            // For this example, just simulate success
            await Task.Delay(100); // Simulate network call
            return true;
        }

        private async Task UpdatePendingChangeAsync(PendingChange change)
        {
            // In a real implementation, update the PendingChange in the database
            await Task.Delay(10); // Simulate db operation
        }

        private async Task DeletePendingChangeAsync(int id)
        {
            // In a real implementation, delete the PendingChange from the database
            await Task.Delay(10); // Simulate db operation
        }

        private async Task PullServerChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Pulling changes from server since: {LastSync}", _lastSyncTime);
                
                // In a real implementation, this would:
                // 1. Call the server API to get changes since LastSyncTime
                // 2. Apply those changes to the local database
                // 3. Handle any merge conflicts
                
                // For this example, simulate a delay for the server call
                await Task.Delay(500, cancellationToken);
                
                _logger.LogInformation("Server changes applied successfully");
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Pull operation cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pulling changes from server");
                throw;
            }
        }

        public async Task QueueChangeAsync<T>(T item, ChangeType changeType) where T : IHasId
        {
            try
            {
                await InitializeAsync();
                
                // Create a pending change entry
                var entityType = typeof(T).Name;
                var entityJson = System.Text.Json.JsonSerializer.Serialize(item);
                
                var pendingChange = new PendingChange
                {
                    EntityType = entityType,
                    EntityId = item.Id,
                    EntityJson = entityJson,
                    ChangeType = changeType,
                    CreatedAt = DateTime.UtcNow,
                    SyncAttempts = 0
                };
                
                // Store the pending change in the database
                var connection = await GetConnectionAsync();
                await connection.InsertAsync(pendingChange);
                
                _logger.LogInformation("Queued {ChangeType} change for {EntityType} with ID {EntityId}", 
                    changeType, entityType, item.Id);
                
                // If online, try to sync immediately
                if (_connectivityService.IsConnected && CurrentState != SyncState.Syncing)
                {
                    _ = SyncAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error queueing change for {EntityType}", typeof(T).Name);
                await _errorHandlingService.HandleExceptionAsync(ex, "QueueChange");
            }
        }

        public async Task<int> GetPendingChangesCountAsync()
        {
            try
            {
                await InitializeAsync();
                
                var connection = await GetConnectionAsync();
                return await connection.Table<PendingChange>().CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending changes count");
                return 0;
            }
        }

        public async Task ClearPendingChangesAsync()
        {
            try
            {
                await InitializeAsync();
                
                var connection = await GetConnectionAsync();
                await connection.DeleteAllAsync<PendingChange>();
                
                _logger.LogInformation("All pending changes cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing pending changes");
                await _errorHandlingService.HandleExceptionAsync(ex, "ClearPendingChanges");
            }
        }

        public async Task StartPeriodicSyncAsync(TimeSpan interval, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting periodic sync with interval: {Interval}", interval);
            
            StopPeriodicSync();
            
            _syncTimer = new System.Timers.Timer(interval.TotalMilliseconds);
            _syncTimer.Elapsed += async (sender, args) =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    StopPeriodicSync();
                    return;
                }
                
                if (_connectivityService.IsConnected && CurrentState != SyncState.Syncing)
                {
                    await SyncAsync(cancellationToken);
                }
            };
            
            _syncTimer.Start();
        }

        public void StopPeriodicSync()
        {
            if (_syncTimer != null)
            {
                _syncTimer.Stop();
                _syncTimer.Dispose();
                _syncTimer = null;
                _logger.LogInformation("Periodic sync stopped");
            }
        }

        private void OnConnectivityChanged(object sender, bool isConnected)
        {
            _logger.LogInformation("Connectivity changed: {IsConnected}", isConnected);
            
            if (isConnected && CurrentState != SyncState.Syncing)
            {
                // When connection is restored, trigger a sync
                _logger.LogInformation("Connection restored, triggering sync");
                _ = SyncAsync();
            }
        }
    }
}