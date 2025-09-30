using System.ComponentModel.DataAnnotations;

namespace HabitTrackerApp.Core.Models;

public class Category
{
    public int Id { get; set; }
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual List<Habit> Habits { get; set; } = new();
}

public class DailyHabitEntry
{
    public int Id { get; set; }
    
    public int HabitId { get; set; }
    public virtual Habit Habit { get; set; } = null!;
    
    public DateTime Date { get; set; }
    public bool IsCompleted { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}