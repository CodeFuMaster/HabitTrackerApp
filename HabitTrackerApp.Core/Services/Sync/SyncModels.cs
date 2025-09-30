namespace HabitTrackerApp.Core.Services.Sync
{
    /// <summary>
    /// Represents a single change record for synchronization
    /// </summary>
    public class SyncRecord
    {
        public int Id { get; set; }
        public required string TableName { get; set; }
        public int RecordId { get; set; }
        public required string Operation { get; set; } // INSERT, UPDATE, DELETE
        public DateTime Timestamp { get; set; }
        public required string Data { get; set; } // JSON serialized data
        public required string DeviceId { get; set; }
        public bool Synced { get; set; }
    }
    
    /// <summary>
    /// Result of a synchronization operation
    /// </summary>
    public class SyncResult
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public int SyncedRecords { get; set; }
        public List<string> Conflicts { get; set; } = new List<string>();
        public DateTime SyncTimestamp { get; set; }
        public List<SyncError> Errors { get; set; } = new List<SyncError>();
        
        public static SyncResult CreateSuccess(int syncedRecords, string message = "Synchronization completed successfully")
        {
            return new SyncResult
            {
                Success = true,
                Message = message,
                SyncedRecords = syncedRecords,
                SyncTimestamp = DateTime.UtcNow
            };
        }
        
        public static SyncResult CreateFailure(string message, List<SyncError>? errors = null)
        {
            return new SyncResult
            {
                Success = false,
                Message = message,
                SyncedRecords = 0,
                SyncTimestamp = DateTime.UtcNow,
                Errors = errors ?? new List<SyncError>()
            };
        }
    }
    
    /// <summary>
    /// Represents a synchronization error
    /// </summary>
    public class SyncError
    {
        public required string ErrorType { get; set; }
        public required string Message { get; set; }
        public string? TableName { get; set; }
        public int? RecordId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Conflict resolution information
    /// </summary>
    public class ConflictResolution
    {
        public int RecordId { get; set; }
        public required string TableName { get; set; }
        public ConflictResolutionStrategy Strategy { get; set; }
        public required string LocalData { get; set; }
        public required string ServerData { get; set; }
        public DateTime LocalTimestamp { get; set; }
        public DateTime ServerTimestamp { get; set; }
    }
    
    /// <summary>
    /// Available conflict resolution strategies
    /// </summary>
    public enum ConflictResolutionStrategy
    {
        LastWriterWins = 0,     // Use the most recent change (recommended for single user)
        ManualResolve = 1,      // Present conflict to user for resolution
        MergeData = 2,          // Attempt to merge non-conflicting fields
        PreferLocal = 3,        // Always prefer local changes
        PreferServer = 4        // Always prefer server changes
    }
    
    /// <summary>
    /// Sync status information
    /// </summary>
    public class SyncStatus
    {
        public bool IsOnline { get; set; }
        public DateTime? LastSyncTime { get; set; }
        public int PendingChanges { get; set; }
        public bool AutoSyncEnabled { get; set; }
        public string? ServerAddress { get; set; }
        public List<string> RecentErrors { get; set; } = new List<string>();
    }
}