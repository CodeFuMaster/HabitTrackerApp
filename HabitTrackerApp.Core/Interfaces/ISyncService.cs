using HabitTrackerApp.Core.Services.Sync;

namespace HabitTrackerApp.Core.Interfaces
{
    /// <summary>
    /// Interface for offline-first synchronization service
    /// Tracks all changes locally and syncs with server when available
    /// </summary>
    public interface ISyncService
    {
        /// <summary>
        /// Track a change to be synchronized later
        /// </summary>
        Task LogChangeAsync(string tableName, int recordId, string operation, object data, string deviceId);
        
        /// <summary>
        /// Attempt to synchronize all pending changes with server
        /// </summary>
        Task<SyncResult> SynchronizeAsync();
        
        /// <summary>
        /// Check if server is available on local network
        /// </summary>
        Task<bool> IsServerAvailableAsync();
        
        /// <summary>
        /// Get server URL if available, null if not found
        /// </summary>
        Task<string?> DiscoverServerAsync();
        
        /// <summary>
        /// Get all pending changes not yet synced
        /// </summary>
        Task<List<SyncRecord>> GetPendingChangesAsync();
        
        /// <summary>
        /// Apply changes received from server
        /// </summary>
        Task<SyncResult> ApplyServerChangesAsync(List<SyncRecord> serverChanges);
        
        /// <summary>
        /// Start background sync service (platform-specific)
        /// </summary>
        Task StartBackgroundSyncAsync();
        
        /// <summary>
        /// Stop background sync service
        /// </summary>
        Task StopBackgroundSyncAsync();
        
        /// <summary>
        /// Force immediate sync attempt
        /// </summary>
        Task<SyncResult> ForceSyncAsync();
    }
}