using System.Text.Json.Serialization;

namespace HabitTrackerMobile.Models;

/// <summary>
/// Mobile app model for Habit - syncs with API
/// </summary>
public class Habit
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("shortDescription")]
    public string ShortDescription { get; set; } = string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("frequency")]
    public HabitFrequency Frequency { get; set; } = HabitFrequency.Daily;

    [JsonPropertyName("targetValue")]
    public decimal? TargetValue { get; set; }

    [JsonPropertyName("targetUnit")]
    public string? TargetUnit { get; set; }

    [JsonPropertyName("categoryId")]
    public int? CategoryId { get; set; }

    [JsonPropertyName("category")]
    public Category? Category { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; } = 1;

    [JsonPropertyName("reminderTime")]
    public TimeSpan? ReminderTime { get; set; }

    [JsonPropertyName("streakCount")]
    public int StreakCount { get; set; }

    [JsonPropertyName("tags")]
    public string Tags { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#512BD4";

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("createdDate")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("lastModified")]
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [JsonPropertyName("dailyEntries")]
    public List<DailyHabitEntry> DailyEntries { get; set; } = new();

    [JsonPropertyName("routineSessions")]
    public List<RoutineSession> RoutineSessions { get; set; } = new();
}

public enum HabitFrequency
{
    Daily = 0,
    Weekly = 1,
    Monthly = 2,
    Custom = 3
}