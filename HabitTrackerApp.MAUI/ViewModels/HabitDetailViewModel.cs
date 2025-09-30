using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTrackerApp.Core.Models;
using HabitTrackerApp.Core.Models.Enhanced;
using HabitTrackerApp.MAUI.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace HabitTrackerApp.MAUI.ViewModels;

[QueryProperty(nameof(HabitId), "HabitId")]
public partial class HabitDetailViewModel : ObservableObject
{
    private readonly IHabitService _habitService;

    [ObservableProperty]
    private Habit? habit;

    [ObservableProperty]
    private RoutineSession? currentSession;

    [ObservableProperty]
    private ObservableCollection<SessionActivity> activities = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isSessionActive;

    [ObservableProperty]
    private string timerDisplay = "00:00";

    private int _habitId;
    private Timer? _timer;
    private DateTime _sessionStartTime;

    public int HabitId
    {
        get => _habitId;
        set
        {
            _habitId = value;
            _ = LoadHabitAsync();
        }
    }

    public HabitDetailViewModel(IHabitService habitService)
    {
        _habitService = habitService;
    }

    private async Task LoadHabitAsync()
    {
        if (HabitId <= 0) return;

        IsLoading = true;
        try
        {
            Habit = await _habitService.GetHabitByIdAsync(HabitId);
            if (Habit != null)
            {
                await LoadTodaysSessionAsync();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load habit: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadTodaysSessionAsync()
    {
        if (Habit == null) return;

        CurrentSession = await _habitService.GetTodaysSessionAsync(Habit.Id);
        if (CurrentSession != null)
        {
            IsSessionActive = !CurrentSession.IsCompleted;
            Activities.Clear();
            foreach (var activity in CurrentSession.Activities ?? new List<SessionActivity>())
            {
                Activities.Add(activity);
            }

            if (IsSessionActive && CurrentSession.StartedAt.HasValue)
            {
                _sessionStartTime = CurrentSession.StartedAt.Value;
                StartTimer();
            }
        }
    }

    [RelayCommand]
    private async Task StartSessionAsync()
    {
        if (Habit == null) return;

        try
        {
            CurrentSession = await _habitService.StartRoutineSessionAsync(Habit.Id);
            IsSessionActive = true;
            _sessionStartTime = DateTime.Now;
            StartTimer();
            
            // Add sample activities for Tuesday Gym routine
            if (Habit.Name?.Contains("Gym") == true)
            {
                await AddGymActivitiesAsync();
            }
            else if (Habit.Name?.Contains("Morning") == true)
            {
                await AddMorningActivitiesAsync();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to start session: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task CompleteSessionAsync()
    {
        if (CurrentSession == null) return;

        try
        {
            await _habitService.CompleteRoutineSessionAsync(CurrentSession.Id);
            IsSessionActive = false;
            StopTimer();
            await LoadTodaysSessionAsync();
            await Shell.Current.DisplayAlert("Success", "Session completed!", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to complete session: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task CompleteActivityAsync(SessionActivity activity)
    {
        if (activity == null) return;

        try
        {
            activity.IsCompleted = !activity.IsCompleted;
            if (activity.IsCompleted && !activity.EndTime.HasValue)
            {
                activity.EndTime = DateTime.Now;
            }
            else if (!activity.IsCompleted)
            {
                activity.EndTime = null;
            }
            
            await _habitService.UpdateSessionActivityAsync(activity);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to update activity: {ex.Message}", "OK");
        }
    }

    private async Task AddGymActivitiesAsync()
    {
        if (CurrentSession == null) return;

        var gymActivities = new[]
        {
            new { Name = "Trap-bar Deadlift", Type = ActivityType.Strength, Duration = 15 },
            new { Name = "Pull-ups", Type = ActivityType.Strength, Duration = 10 },
            new { Name = "Push-ups", Type = ActivityType.Strength, Duration = 8 },
            new { Name = "Farmers Walk", Type = ActivityType.Strength, Duration = 12 }
        };

        foreach (var activity in gymActivities)
        {
            var sessionActivity = new SessionActivity
            {
                RoutineSessionId = CurrentSession.Id,
                Name = activity.Name,
                Type = activity.Type,
                PlannedDuration = TimeSpan.FromMinutes(activity.Duration),
                Order = Activities.Count + 1
            };

            await _habitService.AddSessionActivityAsync(sessionActivity);
            Activities.Add(sessionActivity);
        }
    }

    private async Task AddMorningActivitiesAsync()
    {
        if (CurrentSession == null) return;

        var morningActivities = new[]
        {
            new { Name = "Wim Hof Breathing", Type = ActivityType.Breathing, Duration = 15 },
            new { Name = "Meditation", Type = ActivityType.Meditation, Duration = 20 },
            new { Name = "Cold Shower", Type = ActivityType.Recovery, Duration = 5 }
        };

        foreach (var activity in morningActivities)
        {
            var sessionActivity = new SessionActivity
            {
                RoutineSessionId = CurrentSession.Id,
                Name = activity.Name,
                Type = activity.Type,
                PlannedDuration = TimeSpan.FromMinutes(activity.Duration),
                Order = Activities.Count + 1
            };

            await _habitService.AddSessionActivityAsync(sessionActivity);
            Activities.Add(sessionActivity);
        }
    }

    private void StartTimer()
    {
        _timer = new Timer(UpdateTimer, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }

    private void StopTimer()
    {
        _timer?.Dispose();
        _timer = null;
    }

    private void UpdateTimer(object? state)
    {
        var duration = DateTime.Now - _sessionStartTime;
        TimerDisplay = $"{duration.Hours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
    }
}