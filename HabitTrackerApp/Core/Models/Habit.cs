using System.ComponentModel.DataAnnotations;
using HabitTrackerApp.Core.Models.Enhanced;

namespace HabitTrackerApp.Core.Models;

public class Habit
{
    public int Id { get; set; }
    
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
    
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Enhanced tracking - supports complex routines like gym sessions, morning routine
    /// </summary>
    public bool UseEnhancedTracking { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual List<DailyHabitEntry> DailyEntries { get; set; } = new();
    public virtual List<RoutineSession> RoutineSessions { get; set; } = new();
    public virtual List<ActivityTemplate> ActivityTemplates { get; set; } = new();
}