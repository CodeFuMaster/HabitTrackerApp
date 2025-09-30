using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HabitTrackerApp.Core.Models;

namespace HabitTrackerApp.Core.Models.Enhanced;

/// <summary>
/// Represents a single session of a complex routine (e.g., Tuesday Gym, Morning Routine)
/// Supports your specific use cases: gym workouts, martial arts, meditation with timers
/// </summary>
public class RoutineSession
{
    public int Id { get; set; }
    
    public int HabitId { get; set; }
    public virtual Habit Habit { get; set; } = null!;
    
    [Required]
    public DateTime Date { get; set; }
    
    [MaxLength(200)]
    public string? Name { get; set; } // "Tuesday Gym", "Morning Routine", "BJJ Training"
    
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    
    [NotMapped]
    public TimeSpan? Duration => EndTime.HasValue ? EndTime - StartTime : null;
    
    public bool IsCompleted { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual List<SessionActivity> Activities { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}