using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTrackerApp.Maui.Models;
using HabitTrackerApp.Maui.Services;
using HabitTrackerApp.Maui.Services.Repositories;
using Microsoft.Extensions.Logging;

namespace HabitTrackerApp.Maui.ViewModels
{
    [QueryProperty(nameof(HabitId), "HabitId")]
    public partial class HabitDetailsViewModel : BaseViewModel
    {
        private readonly IHabitRepository _habitRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IDailyHabitEntryRepository _entryRepository;
        private readonly IHabitMetricDefinitionRepository _metricRepository;
        private readonly IDailyMetricValueRepository _metricValueRepository;
        
        [ObservableProperty]
        private int _habitId;
        
        [ObservableProperty]
        private Habit _habit;
        
        [ObservableProperty]
        private Category _category;
        
        [ObservableProperty]
        private ObservableCollection<DailyHabitEntry> _recentEntries;
        
        [ObservableProperty]
        private ObservableCollection<HabitMetricDefinition> _metrics;
        
        [ObservableProperty]
        private double _completionRate;
        
        [ObservableProperty]
        private DateTime _selectedDate;
        
        [ObservableProperty]
        private DailyHabitEntry _selectedEntry;
        
        [ObservableProperty]
        private bool _isEditing;
        
        public HabitDetailsViewModel(
            IHabitRepository habitRepository,
            ICategoryRepository categoryRepository,
            IDailyHabitEntryRepository entryRepository,
            IHabitMetricDefinitionRepository metricRepository,
            IDailyMetricValueRepository metricValueRepository,
            IErrorHandlingService errorHandlingService,
            ILogger<HabitDetailsViewModel> logger) 
            : base(errorHandlingService, logger)
        {
            _habitRepository = habitRepository;
            _categoryRepository = categoryRepository;
            _entryRepository = entryRepository;
            _metricRepository = metricRepository;
            _metricValueRepository = metricValueRepository;
            
            RecentEntries = new ObservableCollection<DailyHabitEntry>();
            Metrics = new ObservableCollection<HabitMetricDefinition>();
            SelectedDate = DateTime.Today;
        }
        
        [RelayCommand]
        private async Task LoadHabitDetails()
        {
            await SafeExecuteAsync(async () =>
            {
                // Load habit details
                Habit = await _habitRepository.GetHabitWithEntriesAsync(HabitId);
                if (Habit == null)
                {
                    ErrorMessage = "Habit not found.";
                    return;
                }
                
                Title = Habit.Name;
                
                // Load category if set
                if (Habit.CategoryId.HasValue)
                {
                    Category = await _categoryRepository.GetByIdAsync(Habit.CategoryId.Value);
                }
                
                // Load recent entries (last 30 days)
                var from = DateTime.Today.AddDays(-30);
                var entries = await _entryRepository.GetEntriesForHabitAsync(HabitId);
                var recentEntries = entries.Where(e => e.Date >= from).OrderByDescending(e => e.Date).ToList();
                
                RecentEntries.Clear();
                foreach (var entry in recentEntries)
                {
                    RecentEntries.Add(entry);
                }
                
                // Load metrics
                var metrics = await _metricRepository.GetMetricsForHabitAsync(HabitId);
                
                Metrics.Clear();
                foreach (var metric in metrics)
                {
                    Metrics.Add(metric);
                }
                
                // Calculate completion rate for the last 4 weeks
                var pastMonth = DateTime.Today.AddDays(-28);
                var validDays = 0;
                var completedDays = 0;
                
                for (var date = pastMonth; date <= DateTime.Today; date = date.AddDays(1))
                {
                    if (Habit.OccursOn(date))
                    {
                        validDays++;
                        if (entries.Any(e => e.Date.Date == date.Date && e.IsCompleted))
                        {
                            completedDays++;
                        }
                    }
                }
                
                CompletionRate = validDays > 0 ? Math.Round((double)completedDays / validDays * 100) : 0;
                
                // Set selected entry for today
                SelectedEntry = entries.FirstOrDefault(e => e.Date.Date == DateTime.Today);
                if (SelectedEntry == null && Habit.OccursOn(DateTime.Today))
                {
                    SelectedEntry = new DailyHabitEntry
                    {
                        HabitId = HabitId,
                        Date = DateTime.Today,
                        IsCompleted = false
                    };
                }
                
                Logger.LogInformation("Loaded habit details for habit: {HabitId}, {HabitName}", 
                    Habit.Id, Habit.Name);
            }, "LoadHabitDetailsOperation");
        }
        
        [RelayCommand]
        private async Task ToggleCompletion()
        {
            await SafeExecuteAsync(async () =>
            {
                if (SelectedEntry == null)
                {
                    SelectedEntry = new DailyHabitEntry
                    {
                        HabitId = HabitId,
                        Date = SelectedDate,
                        IsCompleted = true
                    };
                }
                else
                {
                    SelectedEntry.IsCompleted = !SelectedEntry.IsCompleted;
                }
                
                // Save changes
                await _entryRepository.SaveAsync(SelectedEntry);
                
                // Refresh habit entries
                await LoadHabitDetails();
                
                Logger.LogInformation("Habit completion toggled: {HabitId}, {Date}, {IsCompleted}", 
                    HabitId, SelectedDate.ToString("yyyy-MM-dd"), SelectedEntry.IsCompleted);
            }, "ToggleCompletionOperation");
        }
        
        [RelayCommand]
        private async Task AddReflection(string reflection)
        {
            await SafeExecuteAsync(async () =>
            {
                if (SelectedEntry == null)
                {
                    SelectedEntry = new DailyHabitEntry
                    {
                        HabitId = HabitId,
                        Date = SelectedDate,
                        IsCompleted = false
                    };
                }
                
                SelectedEntry.Reflection = reflection;
                
                // Save changes
                await _entryRepository.SaveAsync(SelectedEntry);
                
                // Refresh habit entries
                await LoadHabitDetails();
                
                IsEditing = false;
                
                Logger.LogInformation("Added reflection for habit: {HabitId}, {Date}", 
                    HabitId, SelectedDate.ToString("yyyy-MM-dd"));
            }, "AddReflectionOperation");
        }
        
        [RelayCommand]
        private async Task SelectDate(DateTime date)
        {
            await SafeExecuteAsync(async () =>
            {
                SelectedDate = date;
                
                // Find entry for the selected date
                var entry = Habit?.DailyHabitEntries.FirstOrDefault(e => e.Date.Date == date.Date);
                
                if (entry != null)
                {
                    SelectedEntry = entry;
                }
                else if (Habit?.OccursOn(date) == true)
                {
                    // Create a new entry if the habit occurs on this date
                    SelectedEntry = new DailyHabitEntry
                    {
                        HabitId = HabitId,
                        Date = date,
                        IsCompleted = false
                    };
                }
                else
                {
                    SelectedEntry = null;
                }
                
                Logger.LogInformation("Selected date for habit: {HabitId}, {Date}", 
                    HabitId, date.ToString("yyyy-MM-dd"));
            }, "SelectDateOperation", false);
        }
        
        [RelayCommand]
        private async Task EditHabit()
        {
            if (Habit == null)
                return;
                
            var parameters = new Dictionary<string, object>
            {
                { "HabitId", HabitId }
            };
            
            await Shell.Current.GoToAsync("HabitEditPage", parameters);
        }
        
        [RelayCommand]
        private async Task DeleteHabit()
        {
            await SafeExecuteAsync(async () =>
            {
                if (Habit == null)
                    return;
                
                bool confirm = await Shell.Current.DisplayAlert(
                    "Delete Habit",
                    $"Are you sure you want to delete '{Habit.Name}'? This action cannot be undone.",
                    "Delete",
                    "Cancel");
                
                if (!confirm)
                    return;
                
                // Soft delete by setting IsDeleted flag
                Habit.IsDeleted = true;
                await _habitRepository.SaveAsync(Habit);
                
                Logger.LogInformation("Deleted habit: {HabitId}, {HabitName}", 
                    Habit.Id, Habit.Name);
                
                // Navigate back
                await Shell.Current.GoToAsync("..");
            }, "DeleteHabitOperation");
        }
    }
}