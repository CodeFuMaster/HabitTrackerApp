namespace HabitTrackerApp.Models
{
    public class DailyMetricValue
    {
        public int Id { get; set; }

        // Foreign key to the daily habit entry
        public int DailyHabitEntryId { get; set; }
        public DailyHabitEntry DailyHabitEntry { get; set; }

        // Reference to the metric definition
        public int? HabitMetricDefinitionId { get; set; }
        public HabitMetricDefinition HabitMetricDefinition { get; set; }

        // Store the user input based on data type
        public double? NumericValue { get; set; }
        public string TextValue { get; set; }
        public bool BooleanValue { get; set; }
    }
}
