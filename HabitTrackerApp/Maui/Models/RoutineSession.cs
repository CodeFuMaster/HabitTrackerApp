using System.Text.Json.Serialization;

namespace HabitTrackerMobile.Models;

/// <summary>
/// Mobile app model for Routine Session - for complex habit tracking
/// </summary>
public class RoutineSession
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("habitId")]
    public int HabitId { get; set; }

    [JsonPropertyName("habit")]
    public Habit? Habit { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public DateTime? EndTime { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan Duration => EndTime?.Subtract(StartTime) ?? TimeSpan.Zero;

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; }

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("activities")]
    public List<SessionActivity> Activities { get; set; } = new();

    // UI Properties (not serialized)
    [JsonIgnore]
    public bool IsRunning => !EndTime.HasValue && StartTime > DateTime.MinValue;

    [JsonIgnore]
    public string FormattedDuration => Duration.ToString(@"hh\:mm\:ss");
}

/// <summary>
/// Mobile app model for Session Activity
/// </summary>
public class SessionActivity
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("routineSessionId")]
    public int RoutineSessionId { get; set; }

    [JsonPropertyName("routineSession")]
    public RoutineSession? RoutineSession { get; set; }

    [JsonPropertyName("activityTemplateId")]
    public int? ActivityTemplateId { get; set; }

    [JsonPropertyName("activityTemplate")]
    public ActivityTemplate? ActivityTemplate { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public DateTime? EndTime { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan Duration => EndTime?.Subtract(StartTime) ?? TimeSpan.Zero;

    [JsonPropertyName("isCompleted")]
    public bool IsCompleted { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("metrics")]
    public List<ActivityMetric> Metrics { get; set; } = new();

    // UI Properties
    [JsonIgnore]
    public string FormattedDuration => Duration.ToString(@"mm\:ss");
}

/// <summary>
/// Mobile app model for Activity Template
/// </summary>
public class ActivityTemplate
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("habitId")]
    public int? HabitId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("estimatedDuration")]
    public TimeSpan? EstimatedDuration { get; set; }

    [JsonPropertyName("instructions")]
    public string Instructions { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public string Tags { get; set; } = string.Empty;

    [JsonPropertyName("defaultMetrics")]
    public List<ActivityMetric> DefaultMetrics { get; set; } = new();
}

/// <summary>
/// Mobile app model for Activity Metric
/// </summary>
public class ActivityMetric
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("sessionActivityId")]
    public int? SessionActivityId { get; set; }

    [JsonPropertyName("sessionActivity")]
    public SessionActivity? SessionActivity { get; set; }

    [JsonPropertyName("templateId")]
    public int? TemplateId { get; set; }

    [JsonPropertyName("template")]
    public ActivityTemplate? Template { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public decimal? Value { get; set; }

    [JsonPropertyName("textValue")]
    public string TextValue { get; set; } = string.Empty;

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("dataType")]
    public MetricDataType DataType { get; set; } = MetricDataType.Number;

    [JsonPropertyName("defaultValue")]
    public string DefaultValue { get; set; } = string.Empty;
}

public enum MetricDataType
{
    Number = 0,
    Text = 1,
    Boolean = 2,
    Time = 3,
    Weight = 4,
    Distance = 5
}