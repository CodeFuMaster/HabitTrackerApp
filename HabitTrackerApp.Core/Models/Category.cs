using System.ComponentModel.DataAnnotations;

namespace HabitTrackerApp.Core.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        // Sync-related fields
        public string? DeviceId { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset LastModifiedDate { get; set; }

        // Navigation property
        public ICollection<Habit> Habits { get; set; } = new List<Habit>();
    }
}