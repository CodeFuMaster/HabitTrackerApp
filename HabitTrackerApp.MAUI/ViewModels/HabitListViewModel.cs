using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTrackerApp.Core.Models;
using HabitTrackerApp.MAUI.Services;
using System.Collections.ObjectModel;

namespace HabitTrackerApp.MAUI.ViewModels;

public partial class HabitListViewModel : ObservableObject
{
    private readonly IHabitService _habitService;

    [ObservableProperty]
    private ObservableCollection<Habit> habits = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool activeFilterSelected = true;

    [ObservableProperty]
    private bool deletedFilterSelected = false;

    [ObservableProperty]
    private bool allFilterSelected = false;

    public HabitListViewModel(IHabitService habitService)
    {
        _habitService = habitService;
        LoadHabitsAsync();
    }

    [RelayCommand]
    private async Task LoadHabitsAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            var habitsList = await _habitService.GetHabitsAsync();
            Habits.Clear();
            foreach (var habit in habitsList)
            {
                Habits.Add(habit);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load habits: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddHabitAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.AddHabitPage));
    }

    [RelayCommand]
    private async Task SelectHabitAsync(Habit habit)
    {
        if (habit == null) return;
        await Shell.Current.GoToAsync($"{nameof(Views.HabitDetailPage)}?HabitId={habit.Id}");
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadHabitsAsync();
    }

    [RelayCommand]
    private void FilterActive()
    {
        ActiveFilterSelected = true;
        DeletedFilterSelected = false;
        AllFilterSelected = false;
        ApplyFilter();
    }

    [RelayCommand]
    private void FilterDeleted()
    {
        ActiveFilterSelected = false;
        DeletedFilterSelected = true;
        AllFilterSelected = false;
        ApplyFilter();
    }

    [RelayCommand]
    private void FilterAll()
    {
        ActiveFilterSelected = false;
        DeletedFilterSelected = false;
        AllFilterSelected = true;
        ApplyFilter();
    }

    [RelayCommand]
    private async Task ViewDetails(Habit habit)
    {
        await Shell.Current.GoToAsync($"{nameof(Views.HabitDetailPage)}?HabitId={habit.Id}");
    }

    [RelayCommand]
    private async Task ViewStatistics(Habit habit)
    {
        await Shell.Current.GoToAsync($"//StatisticsPage?HabitId={habit.Id}");
    }

    [RelayCommand]
    private async Task EditHabit(Habit habit)
    {
        await Shell.Current.GoToAsync($"{nameof(Views.AddHabitPage)}?HabitId={habit.Id}");
    }

    [RelayCommand]
    private async Task DeleteHabit(Habit habit)
    {
        var confirm = await Shell.Current.DisplayAlert("Delete Habit", 
            $"Are you sure you want to delete '{habit.Name}'?", 
            "Delete", "Cancel");
            
        if (confirm)
        {
            try
            {
                IsLoading = true;
                await _habitService.DeleteHabitAsync(habit.Id);
                await LoadHabitsAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to delete habit: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    private async void ApplyFilter()
    {
        await LoadHabitsAsync();
    }
}