using System.ComponentModel.DataAnnotations;
using HabitTrackerApp.Core.Models;

namespace HabitTrackerApp.Core.Models.Enhanced;

public class ActivityTemplate
{
    public int Id { get; set; }
    
    public int? HabitId { get; set; } // Null for global templates
    public virtual Habit? Habit { get; set; }
    
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public ActivityType Type { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public TimeSpan? DefaultDuration { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual List<ActivityMetric> DefaultMetrics { get; set; } = new();
}