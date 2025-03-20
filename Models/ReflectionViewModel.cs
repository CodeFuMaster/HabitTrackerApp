using HabitTrackerApp.Models.Enums;

public class ReflectionViewModel
{
  public int HabitId { get; set; }
  public DateTime Date { get; set; }
  public string ReturnUrl { get; set; }

  public string Reflection { get; set; }
  public int? Score { get; set; }

  // Optionally, store the dynamic metrics for this habit
  public List<MetricInputDto> Metrics { get; set; } = new List<MetricInputDto>();

  public DateTimeOffset CheckedAt { get; set; }
}

public class MetricInputDto
{
  public int MetricDefinitionId { get; set; }
  public string Name { get; set; }
  public string Unit { get; set; }
  public MetricDataType DataType { get; set; }

  // The user’s values:
  public double? UserNumericValue { get; set; }
  public string? UserTextValue { get; set; }
  public bool? UserBoolValue { get; set; }
}