using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using HabitTrackerMobile.Models;
using HabitTrackerMobile.Services;

namespace HabitTrackerMobile.ViewModels;

/// <summary>
/// Main page ViewModel - Dashboard with today's habits
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly ILocalDatabaseService _localDatabase;
    private readonly ISyncService _syncService;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isOnline;

    [ObservableProperty]
    private string welcomeMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<DailyHabitEntry> todayEntries = new();

    [ObservableProperty]
    private ObservableCollection<Habit> activeHabits = new();

    public MainViewModel(IApiService apiService, ILocalDatabaseService localDatabase, ISyncService syncService)
    {
        _apiService = apiService;
        _localDatabase = localDatabase;
        _syncService = syncService;
        
        _syncService.SyncStatusChanged += OnSyncStatusChanged;
        
        UpdateWelcomeMessage();
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        IsLoading = true;
        
        try
        {
            await _localDatabase.InitializeAsync();
            await LoadTodaysDataAsync();
            await _syncService.SyncDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadTodaysDataAsync();
        await _syncService.SyncDataAsync();
    }

    [RelayCommand]
    private async Task ToggleHabitCompletionAsync(DailyHabitEntry entry)
    {
        entry.IsCompleted = !entry.IsCompleted;
        entry.CompletedAt = entry.IsCompleted ? DateTime.Now : null;
        
        await _localDatabase.SaveDailyEntryAsync(entry);
        
        // Update streak if completed
        if (entry.IsCompleted && entry.Habit != null)
        {
            entry.Habit.StreakCount++;
            await _localDatabase.SaveHabitAsync(entry.Habit);
        }
    }

    private async Task LoadTodaysDataAsync()
    {
        var today = DateTime.Today;
        
        // Load habits
        var habits = await _localDatabase.GetHabitsAsync();
        ActiveHabits.Clear();
        foreach (var habit in habits.Where(h => h.IsActive))
        {
            ActiveHabits.Add(habit);
        }

        // Load today's entries
        var entries = await _localDatabase.GetDailyEntriesAsync(today);
        TodayEntries.Clear();
        
        // Create entries for habits that don't have one today
        foreach (var habit in ActiveHabits)
        {
            var existingEntry = entries.FirstOrDefault(e => e.HabitId == habit.Id);
            if (existingEntry == null)
            {
                existingEntry = new DailyHabitEntry
                {
                    HabitId = habit.Id,
                    Habit = habit,
                    Date = today,
                    IsCompleted = false
                };
                await _localDatabase.SaveDailyEntryAsync(existingEntry);
            }
            else
            {
                existingEntry.Habit = habit;
            }
            
            TodayEntries.Add(existingEntry);
        }
    }

    private void UpdateWelcomeMessage()
    {
        var hour = DateTime.Now.Hour;
        WelcomeMessage = hour switch
        {
            < 12 => "Good Morning! 🌅",
            < 17 => "Good Afternoon! ☀️",
            _ => "Good Evening! 🌙"
        };
    }

    private void OnSyncStatusChanged(object? sender, SyncStatusEventArgs e)
    {
        IsOnline = e.IsOnline;
    }
}