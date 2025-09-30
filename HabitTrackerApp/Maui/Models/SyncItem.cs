using SQLite;

namespace HabitTrackerMobile.Models;

/// <summary>
/// Model for tracking changes that need to be synchronized
/// </summary>
[Table("SyncItems")]
public class SyncItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    public int EntityId { get; set; }

    [MaxLength(10)]
    public string Operation { get; set; } = string.Empty; // CREATE, UPDATE, DELETE

    [MaxLength(5000)]
    public string Data { get; set; } = string.Empty; // JSON serialized entity

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsSynced { get; set; } = false;
}