namespace HabitTrackerApp.Models
{
  public class Goal
  {
    public int Id { get; set; }

    // Title of the goal
    public string Title { get; set; }

    // Detailed description
    public string Description { get; set; }

    // Target date for achieving the goal
    public DateTime TargetDate { get; set; }

    // Indicates if the goal has been achieved
    public bool IsAchieved { get; set; }
  }

}
