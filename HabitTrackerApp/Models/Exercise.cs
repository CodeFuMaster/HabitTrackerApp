using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HabitTrackerApp.Models
{
    /// <summary>
    /// Represents a sub-exercise/activity within a parent Habit.
    /// Used for routines like ATG exercises, gym workouts, martial arts drills, etc.
    /// </summary>
    public class Exercise
    {
        public int Id { get; set; }

        // Parent Habit reference
        [Required]
        public int HabitId { get; set; }
        public Habit? Habit { get; set; }

        // Basic Info
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        // Display order in the routine
        public int OrderIndex { get; set; } = 0;

        // Media
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(500)]
        public string? VideoUrl { get; set; }

        [StringLength(500)]
        public string? LocalVideoPath { get; set; }

        // JSON array of resource links
        public string? DocumentUrls { get; set; }

        // Target Metrics (prescribed/template values)
        public int? TargetSets { get; set; }
        public int? TargetReps { get; set; }
        public double? TargetWeight { get; set; } // in kg
        public int? TargetDuration { get; set; } // in seconds
        public int? TargetRPE { get; set; } // Rate of Perceived Exertion 1-10
        public int? RestSeconds { get; set; } // Rest time between sets

        // Metadata
        [StringLength(50)]
        public string? ExerciseType { get; set; } // Strength, Cardio, Flexibility, etc.

        // JSON array of muscle groups
        public string? MuscleGroups { get; set; }

        [StringLength(100)]
        public string? Equipment { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; } // Form cues, tips

        // Status
        public bool IsActive { get; set; } = true;

        // Audit
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedDate { get; set; }

        // Sync
        [StringLength(100)]
        public string? DeviceId { get; set; }

        [StringLength(20)]
        public string SyncStatus { get; set; } = "pending"; // pending, synced, conflict

        // Navigation
        public ICollection<ExerciseLog>? ExerciseLogs { get; set; }
    }
}
