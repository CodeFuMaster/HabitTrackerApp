using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HabitTrackerApp.Core.Models.Enhanced;

namespace HabitTrackerApp.Models.Enhanced
{
    // Represents a complex habit session (e.g., "Tuesday Gym", "Morning Routine")
    public class RoutineSession
    {
        public int Id { get; set; }
        
        [Required]
        public int HabitId { get; set; }
        public Habit Habit { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        // Overall session completion status
        public bool IsCompleted { get; set; }
        
        // Session start/end times
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        // Session notes/reflection
        public string Notes { get; set; }
        
        // Individual exercises/activities within this session
        public ICollection<SessionActivity> Activities { get; set; } = new List<SessionActivity>();
        
        // Overall session rating (1-10)
        public int? Rating { get; set; }
    }
    
    // Individual exercise/activity within a routine session
    public class SessionActivity
    {
        public int Id { get; set; }
        
        [Required]
        public int RoutineSessionId { get; set; }
        public RoutineSession RoutineSession { get; set; }
        
        // Activity template reference (optional)
        public int? ActivityTemplateId { get; set; }
        public ActivityTemplate ActivityTemplate { get; set; }
        
        // Free-form activity name (if not using template)
        [Required]
        public string Name { get; set; }
        
        // Activity type (Exercise, Stretch, Meditation, etc.)
        public ActivityType Type { get; set; }
        
        // Completion status for this specific activity
        public bool IsCompleted { get; set; }
        
        // Activity-specific metrics
        public ICollection<ActivityMetric> Metrics { get; set; } = new List<ActivityMetric>();
        
        // Timer-related fields
        public TimeSpan? Duration { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        // Order within the session
        public int Order { get; set; }
        
        // Activity notes
        public string Notes { get; set; }
    }
    
    // Template for reusable activities (e.g., "Trap-bar Deadlift", "Wim Hof Breathing")
    public class ActivityTemplate
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public ActivityType Type { get; set; }
        
        // Default metrics for this activity template
        public ICollection<ActivityMetricTemplate> DefaultMetrics { get; set; } = new List<ActivityMetricTemplate>();
        
        // Suggested duration
        public TimeSpan? SuggestedDuration { get; set; }
        
        // Instructions or tips
        public string Instructions { get; set; }
        
        // Tags for categorization
        public string Tags { get; set; }
    }
    
    // Specific metric for an activity instance
    public class ActivityMetric
    {
        public int Id { get; set; }
        
        [Required]
        public int SessionActivityId { get; set; }
        public SessionActivity SessionActivity { get; set; }
        
        // Metric template reference
        public int? ActivityMetricTemplateId { get; set; }
        public ActivityMetricTemplate Template { get; set; }
        
        // Metric name (e.g., "Reps", "Weight", "Duration")
        [Required]
        public string Name { get; set; }
        
        // Metric value
        public double? NumericValue { get; set; }
        public string TextValue { get; set; }
        public TimeSpan? TimeValue { get; set; }
        
        // Unit (kg, reps, seconds, etc.)
        public string Unit { get; set; }
        
        // Set number (for exercises with multiple sets)
        public int? SetNumber { get; set; }
    }
    
    // Template for activity metrics
    public class ActivityMetricTemplate
    {
        public int Id { get; set; }
        
        [Required]
        public int ActivityTemplateId { get; set; }
        public ActivityTemplate ActivityTemplate { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        public string Unit { get; set; }
        
        public MetricDataType DataType { get; set; }
        
        // Default value
        public string DefaultValue { get; set; }
        
        // Whether this metric is required
        public bool IsRequired { get; set; }
        
        // Display order
        public int Order { get; set; }
    }
    
    public enum ActivityType
    {
        Exercise = 0,
        Meditation = 1,
        Breathing = 2,
        Stretch = 3,
        Cardio = 4,
        Strength = 5,
        Recovery = 6,
        Mobility = 7,
        Mental = 8,
        Other = 9
    }
}