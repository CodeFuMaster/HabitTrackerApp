using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HabitTrackerApp.Core.Models.Enhanced;

/// <summary>
/// Individual activity within a routine session
/// Examples: "Trap-bar Deadlift", "Wim Hof Breathing", "Cold Shower"
/// </summary>
public class SessionActivity
{
    public int Id { get; set; }
    
    public int RoutineSessionId { get; set; }
    public virtual RoutineSession RoutineSession { get; set; } = null!;
    
    public int? ActivityTemplateId { get; set; }
    public virtual ActivityTemplate? ActivityTemplate { get; set; }
    
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public ActivityType Type { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    /// Planned duration for timed activities (breathing, meditation, cold shower)
    /// </summary>
    public TimeSpan? PlannedDuration { get; set; }
    
    [NotMapped]
    public TimeSpan? ActualDuration => EndTime.HasValue ? EndTime - StartTime : null;
    
    public bool IsCompleted { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public int Order { get; set; } // Order within the session
    
    // Sync-related fields
    public string? DeviceId { get; set; }
    
    // Navigation properties
    public virtual List<ActivityMetric> Metrics { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Activity types for categorization
/// </summary>
public enum ActivityType
{
    Strength,        // Trap-bar DL, Pull-ups, Push-ups
    Cardio,          // Running, cycling
    Breathing,       // Wim Hof breathing
    Meditation,      // Mindfulness, manifestation
    MartialArts,     // BJJ, Wing Chun
    Recovery,        // Cold shower, stretching
    Custom           // Free-form activities
}