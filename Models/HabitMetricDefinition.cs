using System.ComponentModel.DataAnnotations;
using HabitTrackerApp.Models.Enums;

namespace HabitTrackerApp.Models
{
    public class HabitMetricDefinition
    {
        public int Id { get; set; }

        // Reference to which habit this metric belongs
        [Required]
        public int HabitId { get; set; }
        public Habit? Habit { get; set; }

        // The display name (e.g. "Pushups", "Benchpress", "Running distance")
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        // A short label describing the unit (e.g. "reps", "km", "kg", "minutes")
        [StringLength(50)]
        public string Unit { get; set; }

        // Data type handling (e.g. numeric, text, boolean, etc.)
        public MetricDataType DataType { get; set; }
    }
}