using HabitTrackerApp.Core.Data;
using HabitTrackerApp.Core.Models;
using HabitTrackerApp.Core.Models.Enhanced;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HabitTrackerApp.Core.Services;

/// <summary>
/// Offline-first synchronization service for cross-device data sync
/// Handles background sync, conflict resolution, and network connectivity
/// </summary>
public class OfflineSyncService
{
    private readonly DatabaseService _databaseService;
    private readonly HabitTrackerDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly Timer _syncTimer;
    
    public event EventHandler<SyncStatusEventArgs>? SyncStatusChanged;
    public event EventHandler<SyncProgressEventArgs>? SyncProgressChanged;
    
    public bool IsOnline { get; private set; }
    public DateTime LastSyncTime { get; private set; }
    public SyncStatus CurrentStatus { get; private set; } = SyncStatus.Idle;
    
    public OfflineSyncService(DatabaseService databaseService, HabitTrackerDbContext context, HttpClient httpClient)
    {
        _databaseService = databaseService;
        _context = context;
        _httpClient = httpClient;
        _apiBaseUrl = "http://localhost:5000/api"; // Local network API
        
        // Auto-sync every 30 seconds when online
        _syncTimer = new Timer(AutoSync, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        
        CheckConnectivity();
    }
    
    #region Public Sync Methods
    
    /// <summary>
    /// Manual sync trigger - forces immediate synchronization
    /// </summary>
    public async Task<SyncResult> SyncNowAsync()
    {
        if (CurrentStatus == SyncStatus.Syncing)
        {
            return new SyncResult { Success = false, Message = "Sync already in progress" };
        }
        
        try
        {
            await CheckConnectivity();
            
            if (!IsOnline)
            {
                return new SyncResult { Success = false, Message = "No network connection available" };
            }
            
            return await PerformSyncAsync();
        }
        catch (Exception ex)
        {
            OnSyncStatusChanged(SyncStatus.Error, $"Sync failed: {ex.Message}");
            return new SyncResult { Success = false, Message = ex.Message };
        }
    }
    
    /// <summary>
    /// Background auto-sync - runs periodically when online
    /// </summary>
    private async void AutoSync(object? state)
    {
        if (CurrentStatus != SyncStatus.Idle) return;
        
        try
        {
            await CheckConnectivity();
            
            if (IsOnline)
            {
                await PerformSyncAsync();
            }
        }
        catch
        {
            // Ignore auto-sync errors to prevent spam
        }
    }
    
    #endregion
    
    #region Core Sync Logic
    
    private async Task<SyncResult> PerformSyncAsync()
    {
        OnSyncStatusChanged(SyncStatus.Syncing, "Starting synchronization...");
        
        var result = new SyncResult { Success = true };
        var totalOperations = 0;
        var completedOperations = 0;
        
        try
        {
            // Step 1: Get unsynchronized local changes
            var localChanges = await _databaseService.GetUnsyncedRecordsAsync();
            totalOperations = localChanges.Count;
            
            OnSyncProgressChanged(0, totalOperations, "Uploading local changes...");
            
            // Step 2: Push local changes to server
            if (localChanges.Any())
            {
                var pushResult = await PushChangesToServerAsync(localChanges);
                if (pushResult.Success)
                {
                    var syncedIds = localChanges.Select(lc => lc.Id).ToList();
                    await _databaseService.MarkRecordsSyncedAsync(syncedIds);
                    completedOperations = localChanges.Count;
                }
                else
                {
                    result.Success = false;
                    result.Message = pushResult.Message;
                    return result;
                }
            }
            
            OnSyncProgressChanged(completedOperations, totalOperations, "Downloading server changes...");
            
            // Step 3: Pull changes from server since last sync
            var pullResult = await PullChangesFromServerAsync();
            if (!pullResult.Success)
            {
                result.Success = false;
                result.Message = pullResult.Message;
                return result;
            }
            
            // Step 4: Update last sync time
            LastSyncTime = DateTime.UtcNow;
            
            OnSyncStatusChanged(SyncStatus.Completed, $"Sync completed successfully. {localChanges.Count} changes uploaded.");
            
            result.UploadedChanges = localChanges.Count;
            result.DownloadedChanges = pullResult.DownloadedChanges;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = ex.Message;
            OnSyncStatusChanged(SyncStatus.Error, $"Sync failed: {ex.Message}");
        }
        
        return result;
    }
    
    private async Task<SyncResult> PushChangesToServerAsync(List<SyncRecord> changes)
    {
        try
        {
            var payload = new
            {
                DeviceId = changes.First().DeviceId,
                Changes = changes.Select(c => new
                {
                    c.TableName,
                    c.RecordId,
                    Operation = c.Operation.ToString(),
                    c.Data,
                    c.Timestamp
                })
            };
            
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/sync/push", content);
            
            if (response.IsSuccessStatusCode)
            {
                return new SyncResult { Success = true };
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return new SyncResult { Success = false, Message = $"Server error: {error}" };
            }
        }
        catch (Exception ex)
        {
            return new SyncResult { Success = false, Message = $"Push failed: {ex.Message}" };
        }
    }
    
    private async Task<SyncResult> PullChangesFromServerAsync()
    {
        try
        {
            var url = $"{_apiBaseUrl}/sync/pull?since={LastSyncTime:yyyy-MM-ddTHH:mm:ssZ}";
            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var serverChanges = JsonSerializer.Deserialize<ServerSyncResponse>(json);
                
                if (serverChanges?.Changes != null)
                {
                    await ApplyServerChangesAsync(serverChanges.Changes);
                    return new SyncResult { Success = true, DownloadedChanges = serverChanges.Changes.Count };
                }
                
                return new SyncResult { Success = true, DownloadedChanges = 0 };
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return new SyncResult { Success = false, Message = $"Pull failed: {error}" };
            }
        }
        catch (Exception ex)
        {
            return new SyncResult { Success = false, Message = $"Pull failed: {ex.Message}" };
        }
    }
    
    private async Task ApplyServerChangesAsync(List<ServerSyncChange> changes)
    {
        foreach (var change in changes.OrderBy(c => c.Timestamp))
        {
            try
            {
                await ApplyServerChangeAsync(change);
            }
            catch (Exception ex)
            {
                // Log conflict but continue with other changes
                Console.WriteLine($"Conflict applying change {change.TableName}:{change.RecordId} - {ex.Message}");
            }
        }
        
        await _context.SaveChangesAsync();
    }
    
    private async Task ApplyServerChangeAsync(ServerSyncChange change)
    {
        switch (change.TableName.ToLower())
        {
            case "habits":
                await ApplyHabitChangeAsync(change);
                break;
            case "categories":
                await ApplyCategoryChangeAsync(change);
                break;
            case "routinesessions":
                await ApplyRoutineSessionChangeAsync(change);
                break;
            case "sessionactivities":
                await ApplySessionActivityChangeAsync(change);
                break;
            case "activitytemplates":
                await ApplyActivityTemplateChangeAsync(change);
                break;
            // Add more cases as needed
        }
    }
    
    private async Task ApplyHabitChangeAsync(ServerSyncChange change)
    {
        var habit = JsonSerializer.Deserialize<Habit>(change.Data);
        if (habit == null) return;
        
        var existing = await _context.Habits.FindAsync(habit.Id);
        
        switch (change.Operation.ToLower())
        {
            case "insert":
                if (existing == null)
                {
                    _context.Habits.Add(habit);
                }
                break;
            case "update":
                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(habit);
                }
                break;
            case "delete":
                if (existing != null)
                {
                    _context.Habits.Remove(existing);
                }
                break;
        }
    }
    
    private async Task ApplyCategoryChangeAsync(ServerSyncChange change)
    {
        var category = JsonSerializer.Deserialize<Category>(change.Data);
        if (category == null) return;
        
        var existing = await _context.Categories.FindAsync(category.Id);
        
        switch (change.Operation.ToLower())
        {
            case "insert":
                if (existing == null)
                {
                    _context.Categories.Add(category);
                }
                break;
            case "update":
                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(category);
                }
                break;
            case "delete":
                if (existing != null)
                {
                    _context.Categories.Remove(existing);
                }
                break;
        }
    }
    
    private async Task ApplyRoutineSessionChangeAsync(ServerSyncChange change)
    {
        var session = JsonSerializer.Deserialize<RoutineSession>(change.Data);
        if (session == null) return;
        
        var existing = await _context.RoutineSessions.FindAsync(session.Id);
        
        switch (change.Operation.ToLower())
        {
            case "insert":
                if (existing == null)
                {
                    _context.RoutineSessions.Add(session);
                }
                break;
            case "update":
                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(session);
                }
                break;
            case "delete":
                if (existing != null)
                {
                    _context.RoutineSessions.Remove(existing);
                }
                break;
        }
    }
    
    private async Task ApplySessionActivityChangeAsync(ServerSyncChange change)
    {
        var activity = JsonSerializer.Deserialize<SessionActivity>(change.Data);
        if (activity == null) return;
        
        var existing = await _context.SessionActivities.FindAsync(activity.Id);
        
        switch (change.Operation.ToLower())
        {
            case "insert":
                if (existing == null)
                {
                    _context.SessionActivities.Add(activity);
                }
                break;
            case "update":
                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(activity);
                }
                break;
            case "delete":
                if (existing != null)
                {
                    _context.SessionActivities.Remove(existing);
                }
                break;
        }
    }
    
    private async Task ApplyActivityTemplateChangeAsync(ServerSyncChange change)
    {
        var template = JsonSerializer.Deserialize<ActivityTemplate>(change.Data);
        if (template == null) return;
        
        var existing = await _context.ActivityTemplates.FindAsync(template.Id);
        
        switch (change.Operation.ToLower())
        {
            case "insert":
                if (existing == null)
                {
                    _context.ActivityTemplates.Add(template);
                }
                break;
            case "update":
                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(template);
                }
                break;
            case "delete":
                if (existing != null)
                {
                    _context.ActivityTemplates.Remove(existing);
                }
                break;
        }
    }
    
    #endregion
    
    #region Connectivity & Status
    
    private async Task CheckConnectivity()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/ping", 
                new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);
            IsOnline = response.IsSuccessStatusCode;
        }
        catch
        {
            IsOnline = false;
        }
    }
    
    private void OnSyncStatusChanged(SyncStatus status, string message)
    {
        CurrentStatus = status;
        SyncStatusChanged?.Invoke(this, new SyncStatusEventArgs(status, message));
    }
    
    private void OnSyncProgressChanged(int completed, int total, string message)
    {
        SyncProgressChanged?.Invoke(this, new SyncProgressEventArgs(completed, total, message));
    }
    
    #endregion
    
    public void Dispose()
    {
        _syncTimer?.Dispose();
        _httpClient?.Dispose();
    }
}

#region Supporting Classes

public enum SyncStatus
{
    Idle,
    Syncing,
    Completed,
    Error
}

public class SyncResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int UploadedChanges { get; set; }
    public int DownloadedChanges { get; set; }
}

public class SyncStatusEventArgs : EventArgs
{
    public SyncStatus Status { get; }
    public string Message { get; }
    
    public SyncStatusEventArgs(SyncStatus status, string message)
    {
        Status = status;
        Message = message;
    }
}

public class SyncProgressEventArgs : EventArgs
{
    public int Completed { get; }
    public int Total { get; }
    public string Message { get; }
    public double ProgressPercentage => Total > 0 ? (double)Completed / Total * 100 : 0;
    
    public SyncProgressEventArgs(int completed, int total, string message)
    {
        Completed = completed;
        Total = total;
        Message = message;
    }
}

public class ServerSyncResponse
{
    public List<ServerSyncChange> Changes { get; set; } = new();
}

public class ServerSyncChange
{
    public string TableName { get; set; } = string.Empty;
    public int RecordId { get; set; }
    public string Operation { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

#endregion