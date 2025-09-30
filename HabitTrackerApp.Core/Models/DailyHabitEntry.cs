using System;

namespace HabitTrackerApp.Core.Models
{
    public class DailyHabitEntry
    {
        public int Id { get; set; }

        // Foreign key to the related Habit
        public int HabitId { get; set; }
        public required Habit Habit { get; set; }

        // The date of this entry (only the date portion matters)
        public DateTime Date { get; set; }

        // Indicates if the habit was completed on that day
        public bool IsCompleted { get; set; }

        // Optional field for any notes or reflections
        public string? Reflection { get; set; }

        // Record the first time the habit was checked/completed on that day
        public DateTimeOffset CheckedAt { get; set; }

        // Record the last time this entry was modified
        public DateTimeOffset UpdatedAt { get; set; }

        // Score property (e.g., a rating from 1 to 10)
        public int? Score { get; set; }

        // Sync-related fields
        public string? DeviceId { get; set; }

        // Navigation property for metric values
        public ICollection<DailyMetricValue> DailyMetricValues { get; set; } = new List<DailyMetricValue>();
    }
}