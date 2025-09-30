using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTrackerApp.Core.Models;
using HabitTrackerApp.MAUI.Services;
using System.Collections.ObjectModel;

namespace HabitTrackerApp.MAUI.ViewModels;

public partial class DailyViewViewModel : ObservableObject
{
    private readonly IHabitService _habitService;

    // Parameterless constructor for XAML
    public DailyViewViewModel() : this(null!) { }

    public DailyViewViewModel(IHabitService habitService)
    {
        _habitService = habitService;
    }

    [ObservableProperty]
    private DateTime selectedDate = DateTime.Today;

    [ObservableProperty]
    private ObservableCollection<Habit> dailyHabits = [];

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private int completedCount;

    [ObservableProperty]
    private int totalCount;

    public double CompletionRate => TotalCount > 0 ? (double)CompletedCount / TotalCount : 0;

    [RelayCommand]
    private async Task PreviousDay()
    {
        SelectedDate = SelectedDate.AddDays(-1);
        await LoadDailyHabitsAsync();
    }

    [RelayCommand]
    private async Task NextDay()
    {
        SelectedDate = SelectedDate.AddDays(1);
        await LoadDailyHabitsAsync();
    }

    [RelayCommand]
    private async Task ToggleHabit(Habit habit)
    {
        try
        {
            IsLoading = true;
            
            // Check if there's an active session
            var activeSession = await _habitService.GetActiveSessionAsync(habit.Id);
            
            if (activeSession != null)
            {
                // Complete the session
                await _habitService.CompleteSessionAsync(activeSession.Id);
            }
            else
            {
                // Mark habit as completed for today
                var entry = new DailyHabitEntry
                {
                    HabitId = habit.Id,
                    Habit = habit,
                    Date = SelectedDate,
                    IsCompleted = true,
                    CheckedAt = DateTimeOffset.Now,
                    UpdatedAt = DateTimeOffset.Now
                };
                // TODO: Add habit completion recording when service is extended
                // await _habitService.RecordHabitCompletionAsync(entry);
            }

            await LoadDailyHabitsAsync();
        }
        catch (Exception ex)
        {
            // Handle error (show toast, etc.)
            await Shell.Current.DisplayAlert("Error", $"Failed to toggle habit: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadDailyHabitsAsync()
    {
        try
        {
            IsLoading = true;
            var habits = await _habitService.GetHabitsForDateAsync(SelectedDate);
            
            // Filter habits that should appear on the selected date
            var dailyHabits = habits.Where(h => ShouldShowOnDate(h, SelectedDate)).ToList();
            
            // Get today's completed habits using available service methods
            var completedHabitIds = new HashSet<int>();
            foreach (var habit in dailyHabits)
            {
                var completedSessions = await _habitService.GetCompletedSessionsAsync(habit.Id, SelectedDate);
                if (completedSessions.Any())
                {
                    completedHabitIds.Add(habit.Id);
                }
            }
            
            DailyHabits = new ObservableCollection<Habit>(dailyHabits);
            
            // Update stats
            TotalCount = DailyHabits.Count;
            CompletedCount = DailyHabits.Count(h => completedHabitIds.Contains(h.Id));
            OnPropertyChanged(nameof(CompletionRate));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load daily habits: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool ShouldShowOnDate(Habit habit, DateTime date)
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