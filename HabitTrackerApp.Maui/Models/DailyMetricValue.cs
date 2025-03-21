using SQLite;
using HabitTrackerApp.Maui.Services;

namespace HabitTrackerApp.Maui.Models
{
    public class DailyMetricValue : IHasId
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Foreign key to the daily habit entry
        [Indexed]
        public int DailyHabitEntryId { get; set; }
        
        [Ignore]
        public DailyHabitEntry DailyHabitEntry { get; set; }

        // Reference to the metric definition
        [Indexed]
        public int? HabitMetricDefinitionId { get; set; }
        
        [Ignore]
        public HabitMetricDefinition HabitMetricDefinition { get; set; }

        // Store the user input based on data type
        public double? NumericValue { get; set; }
        
        [MaxLength(1000)]
        public string TextValue { get; set; }
        
        public bool BooleanValue { get; set; }
    }
}