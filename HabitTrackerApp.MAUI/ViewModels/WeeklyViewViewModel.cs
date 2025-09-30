using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTrackerApp.Core.Models;
using HabitTrackerApp.MAUI.Services;
using System.Collections.ObjectModel;

namespace HabitTrackerApp.MAUI.ViewModels;

public partial class WeeklyViewViewModel : ObservableObject
{
    private readonly IHabitService _habitService;
    private DateTime _currentWeekStart;

    // Parameterless constructor for XAML
    public WeeklyViewViewModel() : this(null!) { }

    public WeeklyViewViewModel(IHabitService habitService)
    {
        _habitService = habitService;
        _currentWeekStart = GetWeekStart(DateTime.Today);
        UpdateWeekDisplay();
    }

    [ObservableProperty]
    private ObservableCollection<WeeklyHabitItem> weeklyHabits = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string weekTitle = string.Empty;

    [ObservableProperty]
    private string weekDateRange = string.Empty;

    [ObservableProperty]
    private int weeklyCompletedCount;

    [ObservableProperty]
    private int weeklyTotalCount;

    [ObservableProperty]
    private int weeklyStreak;

    public double WeeklyCompletionRate => WeeklyTotalCount > 0 ? (double)WeeklyCompletedCount / WeeklyTotalCount * 100 : 0;



    [RelayCommand]
    private async Task LoadWeeklyDataAsync()
    {
        try
        {
            IsLoading = true;
            
            var startOfWeek = _currentWeekStart;
            var endOfWeek = startOfWeek.AddDays(6);
            
            var allHabits = await _habitService.GetAllHabitsAsync();
            var weeklyItems = new List<WeeklyHabitItem>();
            
            foreach (var habit in allHabits)
            {
                var completedDays = new bool[7];
                
                for (int i = 0; i < 7; i++)
                {
                    var currentDay = startOfWeek.AddDays(i);
                    var sessions = await _habitService.GetCompletedSessionsAsync(habit.Id, currentDay);
                    completedDays[i] = sessions.Any();
                }
                
                weeklyItems.Add(new WeeklyHabitItem
                {
                    Habit = habit,
                    Monday = completedDays[0],
                    Tuesday = completedDays[1],
                    Wednesday = completedDays[2],
                    Thursday = completedDays[3],
                    Friday = completedDays[4],
                    Saturday = completedDays[5],
                    Sunday = completedDays[6],
                    CompletedDays = completedDays.Count(x => x),
                    TotalDays = 7
                });
            }
            
            WeeklyHabits = new ObservableCollection<WeeklyHabitItem>(weeklyItems);
            
            // Update stats
            WeeklyTotalCount = weeklyItems.Sum(x => x.TotalDays);
            WeeklyCompletedCount = weeklyItems.Sum(x => x.CompletedDays);
            
            OnPropertyChanged(nameof(WeeklyCompletionRate));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading weekly data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void PreviousWeek()
    {
        _currentWeekStart = _currentWeekStart.AddDays(-7);
        UpdateWeekDisplay();
        LoadWeeklyDataAsync();
    }

    [RelayCommand]
    private void NextWeek()
    {
        _currentWeekStart = _currentWeekStart.AddDays(7);
        UpdateWeekDisplay();
        LoadWeeklyDataAsync();
    }

    [RelayCommand]
    private void GoToToday()
    {
        _currentWeekStart = GetWeekStart(DateTime.Today);
        UpdateWeekDisplay();
        LoadWeeklyDataAsync();
    }

    private DateTime GetWeekStart(DateTime date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        var mondayOffset = dayOfWeek == 0 ? -6 : 1 - dayOfWeek; // Make Monday the first day
        return date.AddDays(mondayOffset).Date;
    }

    private void UpdateWeekDisplay()
    {
        var endOfWeek = _currentWeekStart.AddDays(6);
        WeekTitle = $"Week of {_currentWeekStart:MMM dd}";
        WeekDateRange = $"{_currentWeekStart:MMM dd} - {endOfWeek:MMM dd, yyyy}";
    }

    public async Task ToggleHabitCompletionAsync(WeeklyHabitItem habitItem, int dayIndex)
    {
        try
        {
            var targetDate = _currentWeekStart.AddDays(dayIndex);
            var sessions = await _habitService.GetCompletedSessionsAsync(habitItem.Habit.Id, targetDate);
            
            if (sessions.Any())
            {
                // Remove completion - this would need additional service method
                // For now, just update UI
                switch (dayIndex)
                {
                    case 0: habitItem.Monday = false; break;
                    case 1: habitItem.Tuesday = false; break;
                    case 2: habitItem.Wednesday = false; break;
                    case 3: habitItem.Thursday = false; break;
                    case 4: habitItem.Friday = false; break;
                    case 5: habitItem.Saturday = false; break;
                    case 6: habitItem.Sunday = false; break;
                }
            }
            else
            {
                // Add completion
                // This would need the service method to create session
                switch (dayIndex)
                {
                    case 0: habitItem.Monday = true; break;
                    case 1: habitItem.Tuesday = true; break;
                    case 2: habitItem.Wednesday = true; break;
                    case 3: habitItem.Thursday = true; break;
                    case 4: habitItem.Friday = true; break;
                    case 5: habitItem.Saturday = true; break;
                    case 6: habitItem.Sunday = true; break;
                }
            }
            
            habitItem.CompletedDays = new[] { habitItem.Monday, habitItem.Tuesday, habitItem.Wednesday, 
                                            habitItem.Thursday, habitItem.Friday, habitItem.Saturday, habitItem.Sunday }
                                            .Count(x => x);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error toggling habit completion: {ex.Message}");
        }
    }
}

public partial class WeeklyHabitItem : ObservableObject
{
    public Habit Habit { get; set; } = new() { Name = "Default" };
    
    [ObservableProperty]
    private bool monday;
    
    [ObservableProperty]
    private bool tuesday;
    
    [ObservableProperty]
    private bool wednesday;
    
    [ObservableProperty]
    private bool thursday;
    
    [ObservableProperty]
    private bool friday;
    
    [ObservableProperty]
    private bool saturday;
    
    [ObservableProperty]
    private bool sunday;
    
    [ObservableProperty]
    private int completedDays;
    
    [ObservableProperty]
    private int totalDays = 7;
    
    public double CompletionRate => TotalDays > 0 ? (double)CompletedDays / TotalDays * 100 : 0;
}