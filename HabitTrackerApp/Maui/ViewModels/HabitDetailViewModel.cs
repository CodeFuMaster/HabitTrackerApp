using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using HabitTrackerMobile.Models;
using HabitTrackerMobile.Services;

namespace HabitTrackerMobile.ViewModels;

/// <summary>
/// Habit Detail ViewModel - Shows detailed habit information and editing
/// </summary>
public partial class HabitDetailViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly ILocalDatabaseService _localDatabase;
    private readonly ISyncService _syncService;

    [ObservableProperty]
    private Habit? currentHabit;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private ObservableCollection<DailyHabitEntry> recentEntries = new();

    [ObservableProperty]
    private ObservableCollection<RoutineSession> recentSessions = new();

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    // Editing properties
    [ObservableProperty]
    private string editName = string.Empty;

    [ObservableProperty]
    private string editDescription = string.Empty;

    [ObservableProperty]
    private string editShortDescription = string.Empty;

    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private string editColor = "#512BD4";

    [ObservableProperty]
    private int editPriority = 1;

    [ObservableProperty]
    private HabitFrequency editFrequency = HabitFrequency.Daily;

    [ObservableProperty]
    private TimeSpan? editReminderTime;

    [ObservableProperty]
    private string editTags = string.Empty;

    public HabitDetailViewModel(IApiService apiService, ILocalDatabaseService localDatabase, ISyncService syncService)
    {
        _apiService = apiService;
        _localDatabase = localDatabase;
        _syncService = syncService;
    }

    [RelayCommand]
    private async Task LoadHabitAsync(int habitId)
    {
        IsLoading = true;
        
        try
        {
            CurrentHabit = await _localDatabase.GetHabitAsync(habitId);
            if (CurrentHabit != null)
            {
                await LoadRecentDataAsync();
                InitializeEditingProperties();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading habit: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void StartEditing()
    {
        IsEditing = true;
        InitializeEditingProperties();
    }

    [RelayCommand]
    private void CancelEditing()
    {
        IsEditing = false;
    }

    [RelayCommand]
    private async Task SaveChangesAsync()
    {
        if (CurrentHabit == null) return;

        CurrentHabit.Name = EditName;
        CurrentHabit.Description = EditDescription;
        CurrentHabit.ShortDescription = EditShortDescription;
        CurrentHabit.CategoryId = SelectedCategory?.Id;
        CurrentHabit.Color = EditColor;
        CurrentHabit.Priority = EditPriority;
        CurrentHabit.Frequency = EditFrequency;
        CurrentHabit.ReminderTime = EditReminderTime;
        CurrentHabit.Tags = EditTags;
        CurrentHabit.LastModified = DateTime.UtcNow;

        await _localDatabase.SaveHabitAsync(CurrentHabit);
        IsEditing = false;
    }

    [RelayCommand]
    private async Task StartRoutineSessionAsync()
    {
        if (CurrentHabit == null) return;

        var newSession = new RoutineSession
        {
            HabitId = CurrentHabit.Id,
            Habit = CurrentHabit,
            Name = $"{CurrentHabit.Name} Session",
            StartTime = DateTime.Now,
            IsCompleted = false,
            Notes = string.Empty
        };

        await _localDatabase.SaveRoutineSessionAsync(newSession);
        RecentSessions.Insert(0, newSession);
        
        // Navigate to routine session page
    }

    [RelayCommand]
    private async Task QuickCompleteAsync()
    {
        if (CurrentHabit == null) return;

        var today = DateTime.Today;
        var todayEntries = await _localDatabase.GetDailyEntriesAsync(today);
        var existingEntry = todayEntries.FirstOrDefault(e => e.HabitId == CurrentHabit.Id);

        if (existingEntry == null)
        {
            existingEntry = new DailyHabitEntry
            {
                HabitId = CurrentHabit.Id,
                Habit = CurrentHabit,
                Date = today,
                IsCompleted = false
            };
        }

        existingEntry.IsCompleted = !existingEntry.IsCompleted;
        existingEntry.CompletedAt = existingEntry.IsCompleted ? DateTime.Now : null;

        await _localDatabase.SaveDailyEntryAsync(existingEntry);

        // Update streak
        if (existingEntry.IsCompleted)
        {
            CurrentHabit.StreakCount++;
            await _localDatabase.SaveHabitAsync(CurrentHabit);
        }

        await LoadRecentDataAsync();
    }

    private async Task LoadRecentDataAsync()
    {
        if (CurrentHabit == null) return;

        // Load recent entries (last 30 days)
        var endDate = DateTime.Today.AddDays(1);
        var startDate = endDate.AddDays(-30);
        
        var allEntries = new List<DailyHabitEntry>();
        for (var date = startDate; date < endDate; date = date.AddDays(1))
        {
            var dayEntries = await _localDatabase.GetDailyEntriesAsync(date);
            var habitEntry = dayEntries.FirstOrDefault(e => e.HabitId == CurrentHabit.Id);
            if (habitEntry != null)
            {
                allEntries.Add(habitEntry);
            }
        }

        RecentEntries.Clear();
        foreach (var entry in allEntries.OrderByDescending(e => e.Date))
        {
            RecentEntries.Add(entry);
        }

        // Load recent sessions
        var sessions = await _localDatabase.GetRoutineSessionsAsync(CurrentHabit.Id);
        RecentSessions.Clear();
        foreach (var session in sessions.Take(10))
        {
            RecentSessions.Add(session);
        }

        // Load categories
        var allCategories = await _localDatabase.GetCategoriesAsync();
        Categories.Clear();
        foreach (var category in allCategories)
        {
            Categories.Add(category);
        }
    }

    private void InitializeEditingProperties()
    {
        if (CurrentHabit == null) return;

        EditName = CurrentHabit.Name;
        EditDescription = CurrentHabit.Description;
        EditShortDescription = CurrentHabit.ShortDescription;
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == CurrentHabit.CategoryId);
        EditColor = CurrentHabit.Color;
        EditPriority = CurrentHabit.Priority;
        EditFrequency = CurrentHabit.Frequency;
        EditReminderTime = CurrentHabit.ReminderTime;
        EditTags = CurrentHabit.Tags;
    }
}