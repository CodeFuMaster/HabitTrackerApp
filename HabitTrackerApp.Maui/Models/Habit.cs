using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using HabitTrackerApp.Maui.Services;

namespace HabitTrackerApp.Maui.Models
{
    public enum RecurrenceType
    {
        Daily,
        Weekly,
        Monthly,
        OneTime
    }

    public class Habit : IHasId
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Basic Info
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(500)]
        public string Description { get; set; }
        
        [MaxLength(200)]
        public string ShortDescription { get; set; }

        // Recurrence
        public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.Daily;

        // For Weekly: comma-separated days (e.g. "Monday,Friday")
        [MaxLength(100)]
        public string WeeklyDays { get; set; }

        // For Monthly: comma-separated numeric days (e.g. "1,15,20")
        [MaxLength(100)]
        public string MonthlyDays { get; set; }

        // For One-time
        public DateTime? SpecificDate { get; set; }

        // Time of Day
        public TimeSpan? TimeOfDay { get; set; }
        public TimeSpan? TimeOfDayEnd { get; set; }

        // Category
        public int? CategoryId { get; set; }
        
        [Ignore]
        public Category Category { get; set; }

        // Tags field (comma-separated)
        [MaxLength(255)]
        public string Tags { get; set; }

        // Image URL
        [MaxLength(255)]
        public string ImageUrl { get; set; }

        // Soft-delete flag
        public bool IsDeleted { get; set; }

        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset LastModifiedDate { get; set; }

        // Navigation property for daily habit entries (handled in repository)
        [Ignore]
        public List<DailyHabitEntry> DailyHabitEntries { get; set; } = new List<DailyHabitEntry>();

        // Helper method to check if the habit occurs on a specific date.
        [Ignore]
        public bool OccursOn(DateTime date)
        {
            switch (RecurrenceType)
            {
                case RecurrenceType.Daily:
                    return true;

                case RecurrenceType.Weekly:
                    if (string.IsNullOrEmpty(WeeklyDays)) return false;
                    var wdays = WeeklyDays.Split(',')
                                        .Select(x => x.Trim())
                                        .Where(x => !string.IsNullOrEmpty(x));
                    return wdays.Contains(date.DayOfWeek.ToString());

                case RecurrenceType.Monthly:
                    if (string.IsNullOrEmpty(MonthlyDays)) return false;
                    var mdays = MonthlyDays.Split(',')
                                         .Select(x => x.Trim())
                                         .Where(x => !string.IsNullOrEmpty(x));
                    return mdays.Contains(date.Day.ToString());

                case RecurrenceType.OneTime:
                    return SpecificDate.HasValue && SpecificDate.Value.Date == date.Date;

                default:
                    return false;
            }
        }

        [Ignore]
        public bool IsCompletedOn(DateTime date)
        {
            return DailyHabitEntries?.Any(e => e.Date.Date == date.Date && e.IsCompleted) ?? false;
        }

        [Ignore]
        public double GetCompletionPercentage(DateTime weekStart)
        {
            var validDays = Enumerable.Range(0, 7)
                .Select(i => weekStart.AddDays(i))
                .Count(d => OccursOn(d));

            if (validDays == 0) return 0;

            var completed = DailyHabitEntries?
                .Count(e => e.Date >= weekStart && e.Date < weekStart.AddDays(7) && e.IsCompleted) ?? 0;

            return Math.Round((completed / (double)validDays) * 100);
        }
    }
}