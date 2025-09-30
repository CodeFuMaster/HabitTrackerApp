namespace HabitTrackerMobile.Services;

/// <summary>
/// Interface for synchronization between local and remote data
/// </summary>
public interface ISyncService
{
    Task<bool> SyncDataAsync();
    Task<bool> PushPendingChangesAsync();
    Task<bool> PullLatestDataAsync();
    Task<bool> IsOnlineAsync();
    
    event EventHandler<SyncStatusEventArgs> SyncStatusChanged;
}

public class SyncStatusEventArgs : EventArgs
{
    public bool IsOnline { get; set; }
    public bool IsSyncing { get; set; }
    public string? Message { get; set; }
    public Exception? Error { get; set; }
}