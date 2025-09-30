using HabitTrackerApp.Models;

public class WeeklyHabitRowModel
{
  public Habit Habit { get; }
  public DateTime WeekStart { get; }
  public List<DateTime> Days { get; }

  public WeeklyHabitRowModel(Habit habit, DateTime weekStart, List<DateTime> days)
  {
    Habit = habit;
    WeekStart = weekStart;
    Days = days;
  }
}