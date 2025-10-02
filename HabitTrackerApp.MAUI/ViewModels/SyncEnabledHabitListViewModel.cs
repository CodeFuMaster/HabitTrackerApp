using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTrackerApp.Core.Models;
using HabitTrackerApp.MAUI.Services;
using HabitTrackerApp.Core.Services.Sync;
using System.Collections.ObjectModel;

namespace HabitTrackerApp.MAUI.ViewModels
{
    /// <summary>
    /// Enhanced ViewModel with offline-first sync capabilities
    /// </summary>
    public partial class SyncEnabledHabitListViewModel : ObservableObject, IDisposable
    {
        private readonly IHabitService _habitService;
        private readonly EnhancedApiService _apiService;
        private readonly SignalRSyncService _signalRService;

        [ObservableProperty]
        private ObservableCollection<Habit> habits = new();

        [ObservableProperty]
        private ObservableCollection<DailyHabitEntry> todayEntries = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isServerOnline;

        [ObservableProperty]
        private string syncStatus = "Offline";

        [ObservableProperty]
        private int connectedDevices;

        [ObservableProperty]
        private DateTime lastSyncTime;

        [ObservableProperty]
        private string serverUrl = "Discovering...";

        [ObservableProperty]
        private bool isSyncing;

        public SyncEnabledHabitListViewModel(
            IHabitService habitService, 
            EnhancedApiService apiService, 
            SignalRSyncService signalRService)
        {
            _habitService = habitService;
            _apiService = apiService;
            _signalRService = signalRService;

            // Subscribe to sync events
            _signalRService.ConnectionStatusChanged += OnConnectionStatusChanged;
            _signalRService.DataChanged += OnDataChanged;
            _signalRService.SyncCompleted += OnSyncCompleted;
            _signalRService.DeviceConnected += OnDeviceConnected;
            _signalRService.DeviceDisconnected += OnDeviceDisconnected;
            _apiService.ServerAvailabilityChanged += OnServerAvailabilityChanged;

            // Initialize
            _ = Task.Run(InitializeAsync);
        }

        private async Task InitializeAsync()
        {
            IsLoading = true;
            
            try
            {
                // Try to connect to sync server
                var connected = await _apiService.InitializeAsync();
                if (connected)
                {
                    ServerUrl = "Connected to sync server";
                    await LoadFromServerAsync();
                }
                else
                {
                    ServerUrl = "Server not found - Working offline";
                    await LoadFromLocalAsync();
                }

                // Load today's entries
                await LoadTodayEntriesAsync();
                
                // Start periodic health checks
                _ = Task.Run(PeriodicHealthCheckAsync);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            if (IsSyncing) return;

            IsSyncing = true;
            try
            {
                if (IsServerOnline)
                {
                    await LoadFromServerAsync();
                }
                else
                {
                    await LoadFromLocalAsync();
                }
                
                await LoadTodayEntriesAsync();
            }
            finally
            {
                IsSyncing = false;
            }
        }

        [RelayCommand]
        private async Task ForceSyncAsync()
        {
            if (IsSyncing) return;

            IsSyncing = true;
            try
            {
                var result = await _apiService.TriggerSyncAsync();
                if (result?.Success == true)
                {
                    SyncStatus = $"Synced {result.SyncedRecords} items";
                    LastSyncTime = DateTime.Now;
                    await RefreshAsync();
                }
                else
                {
                    SyncStatus = result?.Message ?? "Sync failed";
                }
            }
            finally
            {
                IsSyncing = false;
            }
        }

        [RelayCommand]
        private async Task ToggleHabitCompletionAsync(Habit habit)
        {
            try
            {
                var today = DateTime.Today;
                var existingEntry = TodayEntries.FirstOrDefault(e => e.HabitId == habit.Id && e.Date.Date == today);

                if (existingEntry != null)
                {
                    // Toggle completion
                    existingEntry.IsCompleted = !existingEntry.IsCompleted;
                }
                else
                {
                    // Create new entry
                    existingEntry = new DailyHabitEntry
                    {
                        HabitId = habit.Id,
                        Date = today,
                        IsCompleted = true,
                        Habit = habit // Required property
                    };
                    TodayEntries.Add(existingEntry);
                }

                // Try to sync to server if online
                if (IsServerOnline)
                {
                    await _apiService.SaveDailyEntryAsync(existingEntry);
                }

                // Update sync status
                SyncStatus = IsServerOnline ? "Synced" : "Saved locally";
            }
            catch (Exception ex)
            {
                SyncStatus = $"Error: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task AddHabitAsync(string habitName)
        {
            if (string.IsNullOrWhiteSpace(habitName)) return;

            try
            {
                var newHabit = new Habit
                {
                    Name = habitName,
                    IsActive = true
                };

                // Save locally first (offline-first approach)
                var localHabit = await _habitService.CreateHabitAsync(newHabit);
                if (localHabit != null)
                {
                    Habits.Add(localHabit);

                    // Try to sync to server if online
                    if (IsServerOnline)
                    {
                        await _apiService.CreateHabitAsync(localHabit);
                    }

                    SyncStatus = IsServerOnline ? "Synced" : "Saved locally";
                }
            }
            catch (Exception ex)
            {
                SyncStatus = $"Error: {ex.Message}";
            }
        }

        private async Task LoadFromServerAsync()
        {
            try
            {
                var serverHabits = await _apiService.GetHabitsAsync();
                if (serverHabits != null)
                {
                    Habits.Clear();
                    foreach (var habit in serverHabits)
                    {
                        Habits.Add(habit);
                    }
                    SyncStatus = "Loaded from server";
                }
            }
            catch (Exception ex)
            {
                SyncStatus = $"Server error: {ex.Message}";
                await LoadFromLocalAsync();
            }
        }

        private async Task LoadFromLocalAsync()
        {
            try
            {
                var localHabits = await _habitService.GetHabitsAsync();
                Habits.Clear();
                foreach (var habit in localHabits)
                {
                    Habits.Add(habit);
                }
                SyncStatus = "Loaded from local storage";
            }
            catch (Exception ex)
            {
                SyncStatus = $"Local error: {ex.Message}";
            }
        }

        private async Task LoadTodayEntriesAsync()
        {
            try
            {
                var today = DateTime.Today;
                List<DailyHabitEntry>? entries = null;

                // Try server first if online
                if (IsServerOnline)
                {
                    entries = await _apiService.GetDailyEntriesAsync(today);
                }

                // For demo purposes, create empty list if no entries
                if (entries == null)
                {
                    entries = new List<DailyHabitEntry>();
                }

                TodayEntries.Clear();
                foreach (var entry in entries)
                {
                    TodayEntries.Add(entry);
                }
            }
            catch (Exception ex)
            {
                SyncStatus = $"Error loading entries: {ex.Message}";
            }
        }

        private async Task PeriodicHealthCheckAsync()
        {
            while (!_disposed)
            {
                try
                {
                    await _apiService.CheckHealthAsync();
                    await Task.Delay(30000); // Check every 30 seconds
                }
                catch
                {
                    // Ignore errors in health check
                    await Task.Delay(30000);
                }
            }
        }

        // Event Handlers
        private void OnConnectionStatusChanged(bool isConnected)
        {
            IsServerOnline = isConnected;
            SyncStatus = isConnected ? "Online" : "Offline";
            
            if (isConnected)
            {
                _ = Task.Run(async () => await RefreshAsync());
            }
        }

        private void OnDataChanged(SyncRecord syncRecord)
        {
            // Handle real-time data changes from other devices
            _ = Task.Run(async () =>
            {
                switch (syncRecord.TableName.ToLower())
                {
                    case "habits":
                        await RefreshAsync();
                        break;
                    case "dailyhabitentries":
                        await LoadTodayEntriesAsync();
                        break;
                }
                
                SyncStatus = $"Updated from {syncRecord.DeviceId}";
            });
        }

        private void OnSyncCompleted(string message)
        {
            SyncStatus = message;
            LastSyncTime = DateTime.Now;
        }

        private void OnDeviceConnected(string deviceId)
        {
            ConnectedDevices++;
        }

        private void OnDeviceDisconnected(string deviceId)
        {
            ConnectedDevices = Math.Max(0, ConnectedDevices - 1);
        }

        private void OnServerAvailabilityChanged(bool isAvailable)
        {
            IsServerOnline = isAvailable;
            ServerUrl = isAvailable ? "Connected to sync server" : "Server unavailable";
        }

        // Properties for UI binding
        public string DeviceInfo => $"Device: {_signalRService.DeviceId}";
        public bool IsHabitCompleted(int habitId) => TodayEntries.Any(e => e.HabitId == habitId && e.IsCompleted);

        private bool _disposed;
        
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _signalRService.ConnectionStatusChanged -= OnConnectionStatusChanged;
            _signalRService.DataChanged -= OnDataChanged;
            _signalRService.SyncCompleted -= OnSyncCompleted;
            _signalRService.DeviceConnected -= OnDeviceConnected;
            _signalRService.DeviceDisconnected -= OnDeviceDisconnected;
            _apiService.ServerAvailabilityChanged -= OnServerAvailabilityChanged;

            _apiService?.Dispose();
            _signalRService?.Dispose();
        }
    }
}