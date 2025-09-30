using System.ComponentModel.DataAnnotations;

namespace HabitTrackerApp.Core.Models.Enhanced;

/// <summary>
/// Template for reusable activities within habits
/// Examples: "Trap-bar Deadlift", "Wim Hof Breathing", "Cold Shower"
/// </summary>
public class ActivityTemplate
{
    public int Id { get; set; }
    
    public int HabitId { get; set; }
    public virtual Habit Habit { get; set; } = null!;
    
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public ActivityType Type { get; set; }
    
    /// <summary>
    /// Order in which this activity appears in the routine
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// Is this activity required to complete the habit?
    /// </summary>
    public bool IsRequired { get; set; } = true;
    
    /// <summary>
    /// Default duration in minutes (optional)
    /// </summary>
    public int? DefaultDurationMinutes { get; set; }
    
    /// <summary>
    /// JSON string containing activity-specific configuration
    /// e.g., {"sets": 3, "reps": 10, "weight": "bodyweight"}
    /// </summary>
    [MaxLength(2000)]
    public string? Configuration { get; set; }
    
    // Instructions or tips for performing this activity
    [MaxLength(2000)]
    public string? Instructions { get; set; }
    
    // Tags for categorization and search
    public string? Tags { get; set; }
    
    // Sync-related fields
    public string? DeviceId { get; set; }
    
    // Default metrics for this activity template
    public virtual ICollection<ActivityMetricTemplate> DefaultMetrics { get; set; } = new List<ActivityMetricTemplate>();
    
    // Suggested duration for timed activities
    public TimeSpan? SuggestedDuration { get; set; }
    
    // Usage tracking
    public int UsageCount { get; set; }
    public DateTime? LastUsed { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual List<SessionActivity> SessionActivities { get; set; } = new();
}