using System.Text.Json.Serialization;

namespace HabitTrackerMobile.Models;

/// <summary>
/// Mobile app model for Category
/// </summary>
public class Category
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#512BD4";

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("habits")]
    public List<Habit> Habits { get; set; } = new();
}