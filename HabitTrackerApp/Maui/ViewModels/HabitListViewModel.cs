using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using HabitTrackerMobile.Models;
using HabitTrackerMobile.Services;

namespace HabitTrackerMobile.ViewModels;

/// <summary>
/// Habit List ViewModel - Shows all habits with management options
/// </summary>
public partial class HabitListViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly ILocalDatabaseService _localDatabase;
    private readonly ISyncService _syncService;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private ObservableCollection<Habit> habits = new();

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private Habit? selectedHabit;

    [ObservableProperty]
    private string searchText = string.Empty;

    public HabitListViewModel(IApiService apiService, ILocalDatabaseService localDatabase, ISyncService syncService)
    {
        _apiService = apiService;
        _localDatabase = localDatabase;
        _syncService = syncService;
    }

    [RelayCommand]
    private async Task LoadHabitsAsync()
    {
        IsLoading = true;
        
        try
        {
            var allHabits = await _localDatabase.GetHabitsAsync();
            var allCategories = await _localDatabase.GetCategoriesAsync();
            
            Habits.Clear();
            Categories.Clear();
            
            foreach (var habit in allHabits)
            {
                Habits.Add(habit);
            }
            
            foreach (var category in allCategories)
            {
                Categories.Add(category);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading habits: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CreateNewHabitAsync()
    {
        var newHabit = new Habit
        {
            Name = "New Habit",
            Description = "Description for new habit",
            ShortDescription = "New habit",
            IsActive = true,
            Frequency = HabitFrequency.Daily,
            Priority = 1,
            Color = "#512BD4",
            CreatedDate = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        await _localDatabase.SaveHabitAsync(newHabit);
        Habits.Add(newHabit);
        SelectedHabit = newHabit;
    }

    [RelayCommand]
    private async Task DeleteHabitAsync(Habit habit)
    {
        if (habit == null) return;

        var success = await _localDatabase.DeleteHabitAsync(habit.Id);
        if (success)
        {
            Habits.Remove(habit);
            if (SelectedHabit == habit)
            {
                SelectedHabit = null;
            }
        }
    }

    [RelayCommand]
    private async Task ToggleHabitActiveAsync(Habit habit)
    {
        if (habit == null) return;

        habit.IsActive = !habit.IsActive;
        habit.LastModified = DateTime.UtcNow;
        
        await _localDatabase.SaveHabitAsync(habit);
    }

    [RelayCommand]
    private async Task SearchHabitsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadHabitsAsync();
            return;
        }

        var allHabits = await _localDatabase.GetHabitsAsync();
        var filteredHabits = allHabits.Where(h => 
            h.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
            h.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
            h.Tags.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
        ).ToList();

        Habits.Clear();
        foreach (var habit in filteredHabits)
        {
            Habits.Add(habit);
        }
    }

    [RelayCommand]
    private void SelectHabit(Habit habit)
    {
        SelectedHabit = habit;
        // Navigate to habit detail page
    }
}