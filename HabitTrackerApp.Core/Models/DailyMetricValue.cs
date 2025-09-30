using HabitTrackerApp.Core.Models.Enhanced;

namespace HabitTrackerApp.Core.Models
{
    public class DailyMetricValue
    {
        public int Id { get; set; }

        // Foreign key to the daily habit entry
        public int DailyHabitEntryId { get; set; }
        public required DailyHabitEntry DailyHabitEntry { get; set; }

        // Reference to the metric definition
        public int? HabitMetricDefinitionId { get; set; }
        public HabitMetricDefinition? HabitMetricDefinition { get; set; }

        // Store the user input based on data type
        public double? NumericValue { get; set; }
        public string? TextValue { get; set; }
        public bool? BooleanValue { get; set; }
        public TimeSpan? TimeValue { get; set; }

        // Sync-related fields
        public string? DeviceId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset LastModifiedAt { get; set; }
    }
}