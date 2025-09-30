namespace HabitTrackerApp.Core.Models.Enhanced;

public enum MetricDataType
{
    Numeric,    // Integer or decimal values (reps, weight, sets)
    Time,       // Duration values (hold time, rest time)
    Text,       // Text notes or descriptions
    Boolean     // Yes/No, completed/not completed
}