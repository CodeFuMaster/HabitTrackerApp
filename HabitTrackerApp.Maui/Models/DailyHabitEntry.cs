using SQLite;
using System;
using System.Collections.Generic;
using HabitTrackerApp.Maui.Services;

namespace HabitTrackerApp.Maui.Models
{
    public class DailyHabitEntry : IHasId
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Foreign key to the related Habit
        [Indexed]
        public int HabitId { get; set; }
        
        [Ignore]
        public Habit Habit { get; set; }

        // The date of this entry (only the date portion matters)
        [Indexed]
        public DateTime Date { get; set; }

        // Indicates if the habit was completed on that day
        public bool IsCompleted { get; set; }

        // Optional field for any notes or reflections
        [MaxLength(1000)]
        public string Reflection { get; set; }

        // Record the first time the habit was checked/completed on that day
        public DateTimeOffset CheckedAt { get; set; }

        // Record the last time this entry was modified
        public DateTimeOffset UpdatedAt { get; set; }

        // Score property (e.g., a rating from 1 to 10)
        public int? Score { get; set; }

        // Navigation property for metric values
        [Ignore]
        public List<DailyMetricValue> DailyMetricValues { get; set; } = new List<DailyMetricValue>();
    }
}