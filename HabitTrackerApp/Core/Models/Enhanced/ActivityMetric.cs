using System.ComponentModel.DataAnnotations;

namespace HabitTrackerApp.Core.Models.Enhanced;

/// <summary>
/// Activity metric for flexible data tracking (sets, reps, weight, duration)
/// </summary>
public class ActivityMetric
{
    public int Id { get; set; }
    
    // Polymorphic relationship - can belong to either SessionActivity or ActivityTemplate
    public int? SessionActivityId { get; set; }
    public virtual SessionActivity? SessionActivity { get; set; }
    
    public int? ActivityTemplateId { get; set; }
    public virtual ActivityTemplate? ActivityTemplate { get; set; }
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty; // "Sets", "Reps", "Weight", "Hold Time"
    
    public MetricDataType DataType { get; set; }
    
    [MaxLength(20)]
    public string? Unit { get; set; } // "kg", "seconds", "minutes"
    
    // Data storage - use appropriate field based on DataType
    public decimal? NumericValue { get; set; }
    public TimeSpan? TimeValue { get; set; }
    
    [MaxLength(500)]
    public string? TextValue { get; set; }
    
    public bool? BooleanValue { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}