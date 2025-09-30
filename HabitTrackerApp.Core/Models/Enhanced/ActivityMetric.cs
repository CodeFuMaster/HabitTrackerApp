using System.ComponentModel.DataAnnotations;

namespace HabitTrackerApp.Core.Models.Enhanced;

/// <summary>
/// Metrics for tracking specific activity performance
/// Examples: weight lifted, reps completed, duration, etc.
/// </summary>
public class ActivityMetric
{
    public int Id { get; set; }
    
    public int SessionActivityId { get; set; }
    public virtual SessionActivity SessionActivity { get; set; } = null!;
    
    // Metric template reference (optional)
    public int? ActivityMetricTemplateId { get; set; }
    public virtual ActivityMetricTemplate? Template { get; set; }
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., "Weight", "Reps", "Duration"
    
    [Required, MaxLength(50)]
    public string Value { get; set; } = string.Empty; // e.g., "80kg", "10", "5:30"
    
    // Metric values - support multiple data types
    public double? NumericValue { get; set; }
    public string? TextValue { get; set; }
    public TimeSpan? TimeValue { get; set; }
    public bool? BooleanValue { get; set; }
    
    [MaxLength(20)]
    public string? Unit { get; set; } // e.g., "kg", "reps", "minutes"
    
    // Set number (for exercises with multiple sets)
    public int? SetNumber { get; set; }
    
    // Display order within the activity
    public int Order { get; set; }
    
    // Sync-related fields
    public string? DeviceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastModifiedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}