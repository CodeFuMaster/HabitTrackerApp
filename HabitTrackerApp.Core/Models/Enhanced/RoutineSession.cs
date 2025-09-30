using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HabitTrackerApp.Core.Models.Enhanced;

/// <summary>
/// Represents a complex habit session (e.g., "Tuesday Gym", "Morning Routine")
/// This allows tracking individual activities within a habit session
/// </summary>
public class RoutineSession
{
    public int Id { get; set; }
    
    [Required]
    public int HabitId { get; set; }
    public virtual Habit Habit { get; set; } = null!;
    
    [Required]
    public DateTime Date { get; set; }
    
    // Overall session completion status
    public bool IsCompleted { get; set; }
    
    // Session start/end times (for tracking duration)
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Session notes/reflection
    public string? Notes { get; set; }
    
    // Overall session rating (1-10)
    public int? Rating { get; set; }
    
    // Sync-related fields
    public string? DeviceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastModifiedAt { get; set; } = DateTimeOffset.UtcNow;
    
    // Individual exercises/activities within this session
    public virtual ICollection<SessionActivity> Activities { get; set; } = new List<SessionActivity>();
    
    // Calculated properties
    public TimeSpan? Duration => CompletedAt.HasValue && StartedAt.HasValue 
        ? CompletedAt.Value - StartedAt.Value 
        : null;
        
    public int CompletedActivitiesCount => Activities?.Count(a => a.IsCompleted) ?? 0;
    public int TotalActivitiesCount => Activities?.Count ?? 0;
    
    public double CompletionPercentage => TotalActivitiesCount == 0 
        ? 0 
        : Math.Round((CompletedActivitiesCount / (double)TotalActivitiesCount) * 100, 1);
}

/// <summary>
/// Template for activity metrics - defines what metrics an activity template should track
/// </summary>
public class ActivityMetricTemplate
{
    public int Id { get; set; }
    
    [Required]
    public int ActivityTemplateId { get; set; }
    public virtual ActivityTemplate ActivityTemplate { get; set; } = null!;
    
    // Metric name (e.g., "Weight", "Reps", "Duration")
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    // Data type for this metric
    public MetricDataType DataType { get; set; }
    
    // Unit (kg, reps, minutes, etc.)
    [StringLength(20)]
    public string? Unit { get; set; }
    
    // Is this metric required for the activity?
    public bool IsRequired { get; set; }
    
    // Default value (optional)
    public string? DefaultValue { get; set; }
    
    // Display order
    public int Order { get; set; }
    
    // Help text for this metric
    public string? HelpText { get; set; }
    
    // Sync-related fields
    public string? DeviceId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastModifiedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Data types supported for activity metrics
/// </summary>
public enum MetricDataType
{
    Number,
    Text,
    Time,
    Boolean
}