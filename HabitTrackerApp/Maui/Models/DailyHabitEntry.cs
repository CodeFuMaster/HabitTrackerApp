using System.Text.Json.Serialization;

namespace HabitTrackerMobile.Models;

/// <summary>
/// Mobile app model for Daily Habit Entry
/// </summary>
public class DailyHabitEntry
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("habitId")]
    public int HabitId { get; set; }

    [JsonPropertyName("habit")]
    public Habit? Habit { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; }

    [JsonPropertyName("value")]
    public decimal? Value { get; set; }

    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("mood")]
    public int? Mood { get; set; } // 1-5 scale

    [JsonPropertyName("energy")]
    public int? Energy { get; set; } // 1-5 scale

    [JsonPropertyName("reflection")]
    public string? Reflection { get; set; }
}