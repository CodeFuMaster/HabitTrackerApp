using SQLite;
using HabitTrackerApp.Maui.Models.Enums;
using HabitTrackerApp.Maui.Services;

namespace HabitTrackerApp.Maui.Models
{
    public class HabitMetricDefinition : IHasId
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Reference to which habit this metric belongs
        [Indexed]
        public int HabitId { get; set; }
        
        [Ignore]
        public Habit Habit { get; set; }

        // The display name (e.g. "Pushups", "Benchpress", "Running distance")
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        // A short label describing the unit (e.g. "reps", "km", "kg", "minutes")
        [MaxLength(50)]
        public string Unit { get; set; }

        // Data type handling (e.g. numeric, text, boolean, etc.)
        public MetricDataType DataType { get; set; }
    }
}