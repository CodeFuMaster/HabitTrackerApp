using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace HabitTrackerApp.Core.Models;

public enum SyncOperation
{
    Insert,
    Update,
    Delete
}

/// <summary>
/// Tracks changes for offline-first synchronization
/// </summary>
public class SyncRecord
{
    public int Id { get; set; }
    
    [Required, MaxLength(100)]
    public string TableName { get; set; } = string.Empty;
    
    public int RecordId { get; set; }
    
    public SyncOperation Operation { get; set; }
    
    [Required]
    public string Data { get; set; } = string.Empty; // JSON serialized data
    
    [Required, MaxLength(100)]
    public string DeviceId { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public bool IsSynced { get; set; } = false;
}

/// <summary>
/// Device information for multi-device sync
/// </summary>
public class DeviceInfo
{
    public int Id { get; set; }
    
    [Required, MaxLength(100)]
    public string DeviceId { get; set; } = string.Empty;
    
    [Required, MaxLength(200)]
    public string DeviceName { get; set; } = string.Empty;
    
    [Required, MaxLength(50)]
    public string Platform { get; set; } = string.Empty; // iOS, Android, Windows, macOS
    
    public DateTime LastSyncTime { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}