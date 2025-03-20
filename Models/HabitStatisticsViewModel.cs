public class HabitStatisticsViewModel
{
  public int HabitId { get; set; }
  public string HabitName { get; set; }

  public int TotalCompletions { get; set; }
  public double CompletionRate { get; set; }
  public int Last7DaysCompletions { get; set; }
  public int CurrentStreak { get; set; }
  public int LongestStreak { get; set; }

  public List<MonthlyCompletionDto> MonthlyCompletions { get; set; }
  public List<WeeklyCompletionDto> WeeklyCompletionsList { get; set; }
  public int WeeklyCompletions { get; set; }
  public int TrendDifference { get; set; }
  public double AverageScore { get;  set; }
  public int DaysSinceLastMiss { get;  set; }
  public int LongestGap { get; set; }
}

public class MonthlyCompletionDto
{
  public int Year { get; set; }
  public int Month { get; set; }
  public int CompletionCount { get; set; }
}

public class WeeklyCompletionDto
{
  public int Year { get; set; }
  public int WeekNumber { get; set; }
  public int CompletionCount { get; set; }
}
