using SQLite;
using HabitTrackerApp.Maui.Services;

namespace HabitTrackerApp.Maui.Models
{
    public class Category : IHasId
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(255)]
        public string ImageUrl { get; set; }

        // SQLite doesn't directly support navigation properties like EF Core
        // We'll handle relationships in our repository layer
        [Ignore]
        public List<Habit> Habits { get; set; } = new List<Habit>();
    }

    // SQLite-friendly attribute for validation
    public class RequiredAttribute : Attribute { }

    // SQLite-friendly attribute for string length
    public class MaxLengthAttribute : Attribute
    {
        public int Length { get; set; }
        public MaxLengthAttribute(int length) => Length = length;
    }
}