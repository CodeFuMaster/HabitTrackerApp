namespace HabitTrackerApp.Core.Services.Sync
{
    public interface IOfflineSyncService
    {
        /// <summary>
        /// Check if the local network server is available for synchronization
        /// </summary>
        Task<bool> IsServerAvailableAsync();
        
        /// <summary>
        /// Perform a complete synchronization with the server
        /// </summary>
        Task<SyncResult> SyncWithServerAsync();
        
        /// <summary>
        /// Sync specific data type and record
        /// </summary>
        Task<SyncResult> SyncSpecificDataAsync(string dataType, int recordId);
        
        /// <summary>
        /// Enable or disable automatic background synchronization
        /// </summary>
        Task EnableAutoSyncAsync(bool enabled);
        
        /// <summary>
        /// Get the timestamp of the last successful synchronization
        /// </summary>
        Task<DateTime> GetLastSyncTimestampAsync();
        
        /// <summary>
        /// Force push all local changes to server (useful for manual sync)
        /// </summary>
        Task<SyncResult> PushAllChangesAsync();
        
        /// <summary>
        /// Pull all changes from server since last sync
        /// </summary>
        Task<SyncResult> PullAllChangesAsync();
        
        /// <summary>
        /// Get count of pending changes waiting to be synced
        /// </summary>
        Task<int> GetPendingChangesCountAsync();
    }
    
    public interface INetworkDiscoveryService
    {
        /// <summary>
        /// Discover the HabitTracker server on the local network
        /// </summary>
        Task<string?> DiscoverServerAsync();
        
        /// <summary>
        /// Test connection to a specific server address
        /// </summary>
        Task<bool> TestServerConnectionAsync(string serverAddress);
        
        /// <summary>
        /// Get list of potential server addresses on the network
        /// </summary>
        Task<List<string>> ScanNetworkForServersAsync();
    }
    
    public interface ILocalDataService
    {
        /// <summary>
        /// Initialize the local SQLite database
        /// </summary>
        Task InitializeDatabaseAsync();
        
        /// <summary>
        /// Get all local changes since a specific timestamp
        /// </summary>
        Task<List<SyncRecord>> GetLocalChangesSinceAsync(DateTime timestamp);
        
        /// <summary>
        /// Apply server changes to local database
        /// </summary>
        Task ApplyServerChangesAsync(List<SyncRecord> changes);
        
        /// <summary>
        /// Log a local change for future synchronization
        /// </summary>
        Task LogLocalChangeAsync(string tableName, int recordId, string operation, object data);
        
        /// <summary>
        /// Mark changes as synchronized
        /// </summary>
        Task MarkChangesSyncedAsync(List<int> syncLogIds);
        
        /// <summary>
        /// Clean up old sync logs (older than specified days)
        /// </summary>
        Task CleanupOldSyncLogsAsync(int daysToKeep = 30);
    }
}