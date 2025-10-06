using System;
using System.ComponentModel.DataAnnotations;

namespace HabitTrackerApp.Models
{
    /// <summary>
    /// Represents a single set/performance of an exercise on a specific date.
    /// Used to track actual workout performance vs. target.
    /// </summary>
    public class ExerciseLog
    {
        public int Id { get; set; }

        // References
        [Required]
        public int ExerciseId { get; set; }
        public Exercise? Exercise { get; set; }

        [Required]
        public int DailyHabitEntryId { get; set; }
        public DailyHabitEntry? DailyHabitEntry { get; set; }

        // Date
        [Required]
        public DateTime Date { get; set; }

        // Performance Data
        public int SetNumber { get; set; } = 1; // 1st set, 2nd set, etc.
        public int? ActualReps { get; set; }
        public double? ActualWeight { get; set; } // in kg
        public int? ActualDuration { get; set; } // in seconds
        public int? ActualRPE { get; set; } // Rate of Perceived Exertion 1-10

        // Timing
        public DateTime? CompletedAt { get; set; }

        // Notes
        [StringLength(500)]
        public string? Notes { get; set; }

        // Sync
        [StringLength(100)]
        public string? DeviceId { get; set; }

        [StringLength(20)]
        public string SyncStatus { get; set; } = "pending"; // pending, synced, conflict
    }
}
