using CommunityToolkit.Mvvm.ComponentModel;
using HabitTrackerApp.Core.Models;
using HabitTrackerApp.MAUI.Services;
using System.Collections.ObjectModel;

namespace HabitTrackerApp.MAUI.ViewModels;

public partial class StatisticsViewModel : ObservableObject
{
    private readonly IHabitService _habitService;

    // Parameterless constructor for XAML
    public StatisticsViewModel() : this(null!) { }

    public StatisticsViewModel(IHabitService habitService)
    {
        _habitService = habitService;
    }

    [ObservableProperty]
    private int totalHabits;

    [ObservableProperty]
    private int activeHabits;

    [ObservableProperty]
    private double overallCompletionRate;

    [ObservableProperty]
    private int bestStreak;

    [ObservableProperty]
    private int monthlyCompletedSessions;

    [ObservableProperty]
    private int monthlyStreak;

    [ObservableProperty]
    private ObservableCollection<RecentActivityItem> recentActivity = [];

    [ObservableProperty]
    private ObservableCollection<TopHabitItem> topHabits = [];

    [ObservableProperty]
    private bool isLoading;



    private async Task LoadStatisticsAsync()
    {
        try
        {
            IsLoading = true;
            
            // Load basic stats
            var allHabits = await _habitService.GetAllHabitsAsync();
            TotalHabits = allHabits.Count;
            ActiveHabits = allHabits.Count(h => !h.IsDeleted);

            // Calculate overall completion rate
            await CalculateCompletionRate(allHabits);

            // Load recent activity
            await LoadRecentActivity();

            // Load top performing habits
            await LoadTopHabits(allHabits);

            // Calculate monthly stats
            await CalculateMonthlyStats();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load statistics: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CalculateCompletionRate(List<Habit> habits)
    {
        var totalExpected = 0;
        var totalCompleted = 0;
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-30); // Last 30 days

        foreach (var habit in habits.Where(h => !h.IsDeleted))
        {
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (ShouldHabitRunOnDate(habit, date))
                {
                    totalExpected++;
                    var sessions = await _habitService.GetCompletedSessionsAsync(habit.Id, date);
                    if (sessions.Any())
                    {
                        totalCompleted++;
                    }
                }
            }
        }

        OverallCompletionRate = totalExpected > 0 ? (double)totalCompleted / totalExpected : 0;
    }

    private async Task LoadRecentActivity()
    {
        try
        {
            var recentItems = new List<RecentActivityItem>();
            var allHabits = await _habitService.GetAllHabitsAsync();
            
            // Get recent sessions from all habits
            foreach (var habit in allHabits.Where(h => !h.IsDeleted).Take(10))
            {
                var recentSessions = await _habitService.GetRecentSessionsAsync(habit.Id, 5);
                foreach (var session in recentSessions.Where(s => s.IsCompleted))
                {
                    recentItems.Add(new RecentActivityItem
                    {
                        HabitName = habit.Name,
                        Description = "Completed session",
                        CompletedAt = session.CompletedAt ?? session.StartedAt ?? DateTime.Now
                    });
                }
            }

            // Sort by most recent and take top 10
            var sortedItems = recentItems
                .OrderByDescending(r => r.CompletedAt)
                .Take(10)
                .ToList();

            RecentActivity = new ObservableCollection<RecentActivityItem>(sortedItems);
        }
        catch (Exception ex)
        {
            // Handle error silently for now
            RecentActivity = new ObservableCollection<RecentActivityItem>();
        }
    }

    private async Task LoadTopHabits(List<Habit> habits)
    {
        try
        {
            var topHabitItems = new List<TopHabitItem>();
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-30); // Last 30 days

            foreach (var habit in habits.Where(h => !h.IsDeleted))
            {
                var expected = 0;
                var completed = 0;

                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    if (ShouldHabitRunOnDate(habit, date))
                    {
                        expected++;
                        var sessions = await _habitService.GetCompletedSessionsAsync(habit.Id, date);
                        if (sessions.Any())
                        {
                            completed++;
                        }
                    }
                }

                if (expected > 0)
                {
                    topHabitItems.Add(new TopHabitItem
                    {
                        Name = habit.Name,
                        Category = habit.Category ?? new Category { Name = "No Category" },
                        CompletionRate = (double)completed / expected
                    });
                }
            }

            // Sort by completion rate and assign ranks
            var sortedHabits = topHabitItems
                .OrderByDescending(h => h.CompletionRate)
                .Take(5)
                .Select((h, index) => { h.Rank = index + 1; return h; })
                .ToList();

            TopHabits = new ObservableCollection<TopHabitItem>(sortedHabits);
        }
        catch (Exception ex)
        {
            // Handle error silently for now
            TopHabits = new ObservableCollection<TopHabitItem>();
        }
    }

    private async Task CalculateMonthlyStats()
    {
        try
        {
            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            
            var allHabits = await _habitService.GetAllHabitsAsync();
            var completedSessions = 0;

            foreach (var habit in allHabits.Where(h => !h.IsDeleted))
            {
                for (var date = startOfMonth; date <= DateTime.Today && date <= endOfMonth; date = date.AddDays(1))
                {
                    var sessions = await _habitService.GetCompletedSessionsAsync(habit.Id, date);
                    if (sessions.Any())
                    {
                        completedSessions++;
                    }
                }
            }

            MonthlyCompletedSessions = completedSessions;
            
            // Calculate current streak (simplified)
            MonthlyStreak = await CalculateCurrentStreak(allHabits);
        }
        catch (Exception ex)
        {
            MonthlyCompletedSessions = 0;
            MonthlyStreak = 0;
        }
    }

    private async Task<int> CalculateCurrentStreak(List<Habit> habits)
    {
        var streak = 0;
        var date = DateTime.Today;
        
        // Look back day by day until we find a day with no completions
        while (date >= DateTime.Today.AddDays(-365)) // Max 1 year lookback
        {
            var hasAnyCompletion = false;
            
            foreach (var habit in habits.Where(h => !h.IsDeleted))
            {
                if (ShouldHabitRunOnDate(habit, date))
                {
                    var sessions = await _habitService.GetCompletedSessionsAsync(habit.Id, date);
                    if (sessions.Any())
                    {
                        hasAnyCompletion = true;
                        break;
                    }
                }
            }
            
            if (hasAnyCompletion)
            {
                streak++;
                date = date.AddDays(-1);
            }
            else
            {
                break;
            }
        }
        
        return streak;
    }

    private bool ShouldHabitRunOnDate(Habit habit, DateTime date)
    {
        return habit.RecurrenceType switch
        {
            RecurrenceType.Daily => true,
            RecurrenceType.Weekly => IsWeeklyHabitActiveOnDate(habit, date),
            RecurrenceType.OneTime => habit.SpecificDate?.Date == date.Date,
            _ => false
        };
    }

    private bool IsWeeklyHabitActiveOnDate(Habit habit, DateTime date)
    {
        if (string.IsNullOrEmpty(habit.WeeklyDays))
            return false;

        var dayOfWeek = date.DayOfWeek.ToString();
        return habit.WeeklyDays.Contains(dayOfWeek, StringComparison.OrdinalIgnoreCase);
    }
}

public class RecentActivityItem
{
    public string HabitName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
}

public class TopHabitItem
{
    public int Rank { get; set; }
    public string Name { get; set; } = string.Empty;
    public Category Category { get; set; } = new() { Name = "Default" };
    public double CompletionRate { get; set; }
}