using System.ComponentModel.DataAnnotations;

namespace HabitTrackerApp.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(255)]
        public string ImageUrl { get; set; }

        // Navigation property
        public ICollection<Habit> Habits { get; set; }
    }
}