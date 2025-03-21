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
    public partial class HabitEditViewModel : BaseViewModel
    {
        private readonly IHabitRepository _habitRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IHabitMetricDefinitionRepository _metricRepository;
        
        [ObservableProperty]
        private int _habitId;
        
        [ObservableProperty]
        private string _name;
        
        [ObservableProperty]
        private string _description;
        
        [ObservableProperty]
        private string _shortDescription;
        
        [ObservableProperty]
        private RecurrenceType _recurrenceType = RecurrenceType.Daily;
        
        [ObservableProperty]
        private string _weeklyDays;
        
        [ObservableProperty]
        private string _monthlyDays;
        
        [ObservableProperty]
        private DateTime? _specificDate;
        
        [ObservableProperty]
        private TimeSpan? _timeOfDay;
        
        [ObservableProperty]
        private TimeSpan? _timeOfDayEnd;
        
        [ObservableProperty]
        private int? _selectedCategoryId;
        
        [ObservableProperty]
        private ObservableCollection<Category> _categories;
        
        [ObservableProperty]
        private ObservableCollection<HabitMetricDefinition> _metrics;
        
        [ObservableProperty]
        private string _tags;
        
        [ObservableProperty]
        private string _imageUrl;
        
        [ObservableProperty]
        private bool _isNewHabit;
        
        [ObservableProperty]
        private string _weekDaysSelected;
        
        public HabitEditViewModel(
            IHabitRepository habitRepository,
            ICategoryRepository categoryRepository,
            IHabitMetricDefinitionRepository metricRepository,
            IErrorHandlingService errorHandlingService,
            ILogger<HabitEditViewModel> logger) 
            : base(errorHandlingService, logger)
        {
            _habitRepository = habitRepository;
            _categoryRepository = categoryRepository;
            _metricRepository = metricRepository;
            
            Categories = new ObservableCollection<Category>();
            Metrics = new ObservableCollection<HabitMetricDefinition>();
            IsNewHabit = true;
        }
        
        [RelayCommand]
        private async Task LoadData()
        {
            await SafeExecuteAsync(async () =>
            {
                // Load categories
                var categories = await _categoryRepository.GetAllAsync();
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
                
                // If we have a habit ID, load the habit for editing
                if (HabitId > 0)
                {
                    var habit = await _habitRepository.GetByIdAsync(HabitId);
                    if (habit != null)
                    {
                        // Populate view model properties from the habit
                        IsNewHabit = false;
                        Title = "Edit Habit";
                        
                        Name = habit.Name;
                        Description = habit.Description;
                        ShortDescription = habit.ShortDescription;
                        RecurrenceType = habit.RecurrenceType;
                        WeeklyDays = habit.WeeklyDays;
                        MonthlyDays = habit.MonthlyDays;
                        SpecificDate = habit.SpecificDate;
                        TimeOfDay = habit.TimeOfDay;
                        TimeOfDayEnd = habit.TimeOfDayEnd;
                        SelectedCategoryId = habit.CategoryId;
                        Tags = habit.Tags;
                        ImageUrl = habit.ImageUrl;
                        
                        // Load metrics for this habit
                        var metrics = await _metricRepository.GetMetricsForHabitAsync(HabitId);
                        Metrics.Clear();
                        foreach (var metric in metrics)
                        {
                            Metrics.Add(metric);
                        }
                        
                        Logger.LogInformation("Loaded habit for editing: {HabitId}, {HabitName}", 
                            habit.Id, habit.Name);
                    }
                    else
                    {
                        ErrorMessage = "Habit not found.";
                    }
                }
                else
                {
                    // New habit
                    IsNewHabit = true;
                    Title = "Add Habit";
                    
                    // Set defaults
                    RecurrenceType = RecurrenceType.Daily;
                }
                
                // Format UI-friendly representation of weekly days
                UpdateWeekDaysSelected();
            }, "LoadDataOperation");
        }
        
        [RelayCommand]
        private async Task Save()
        {
            await SafeExecuteAsync(async () =>
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(Name))
                {
                    ErrorMessage = "Habit name is required.";
                    return;
                }
                
                // Create or update habit
                Habit habit;
                if (IsNewHabit)
                {
                    habit = new Habit
                    {
                        CreatedDate = DateTimeOffset.Now
                    };
                }
                else
                {
                    habit = await _habitRepository.GetByIdAsync(HabitId);
                    if (habit == null)
                    {
                        ErrorMessage = "Habit not found.";
                        return;
                    }
                }
                
                // Update habit properties
                habit.Name = Name?.Trim();
                habit.Description = Description?.Trim();
                habit.ShortDescription = ShortDescription?.Trim();
                habit.RecurrenceType = RecurrenceType;
                habit.WeeklyDays = WeeklyDays?.Trim();
                habit.MonthlyDays = MonthlyDays?.Trim();
                habit.SpecificDate = SpecificDate;
                habit.TimeOfDay = TimeOfDay;
                habit.TimeOfDayEnd = TimeOfDayEnd;
                habit.CategoryId = SelectedCategoryId;
                habit.Tags = Tags?.Trim();
                habit.ImageUrl = ImageUrl?.Trim();
                habit.LastModifiedDate = DateTimeOffset.Now;
                
                // Save changes
                if (IsNewHabit)
                {
                    habit.Id = await _habitRepository.SaveAsync(habit);
                    HabitId = habit.Id; // Update with the newly created ID
                    
                    Logger.LogInformation("Created new habit: {HabitId}, {HabitName}", 
                        habit.Id, habit.Name);
                }
                else
                {
                    await _habitRepository.SaveAsync(habit);
                    
                    Logger.LogInformation("Updated habit: {HabitId}, {HabitName}", 
                        habit.Id, habit.Name);
                }
                
                // Navigate back
                await Shell.Current.GoToAsync("..");
            }, "SaveHabitOperation");
        }
        
        [RelayCommand]
        private async Task Cancel()
        {
            // Navigate back without saving
            await Shell.Current.GoToAsync("..");
        }
        
        [RelayCommand]
        private async Task AddMetric()
        {
            await SafeExecuteAsync(async () =>
            {
                if (HabitId <= 0 && IsNewHabit)
                {
                    ErrorMessage = "Please save the habit first before adding metrics.";
                    return;
                }
                
                // Create a new metric
                var metric = new HabitMetricDefinition
                {
                    HabitId = HabitId,
                    Name = "New Metric",
                    Unit = "",
                    DataType = Enums.MetricDataType.Numeric
                };
                
                // Save to repository
                await _metricRepository.SaveAsync(metric);
                
                // Add to UI collection
                Metrics.Add(metric);
                
                Logger.LogInformation("Added new metric for habit: {HabitId}", HabitId);
            }, "AddMetricOperation");
        }
        
        [RelayCommand]
        private async Task UpdateMetric(HabitMetricDefinition metric)
        {
            await SafeExecuteAsync(async () =>
            {
                if (metric == null)
                    return;
                
                // Save updated metric to repository
                await _metricRepository.SaveAsync(metric);
                
                Logger.LogInformation("Updated metric: {MetricId}, {MetricName} for habit: {HabitId}", 
                    metric.Id, metric.Name, HabitId);
            }, "UpdateMetricOperation");
        }
        
        [RelayCommand]
        private async Task DeleteMetric(HabitMetricDefinition metric)
        {
            await SafeExecuteAsync(async () =>
            {
                if (metric == null)
                    return;
                
                bool confirm = await Shell.Current.DisplayAlert(
                    "Delete Metric",
                    $"Are you sure you want to delete the metric '{metric.Name}'?",
                    "Delete",
                    "Cancel");
                
                if (!confirm)
                    return;
                
                // Delete from repository
                await _metricRepository.DeleteAsync(metric);
                
                // Remove from UI collection
                Metrics.Remove(metric);
                
                Logger.LogInformation("Deleted metric: {MetricId}, {MetricName} for habit: {HabitId}", 
                    metric.Id, metric.Name, HabitId);
            }, "DeleteMetricOperation");
        }
        
        [RelayCommand]
        private async Task ToggleWeekDay(string day)
        {
            var days = WeeklyDays?.Split(',').Select(d => d.Trim()).ToList() ?? new List<string>();
            
            if (days.Contains(day))
            {
                days.Remove(day);
            }
            else
            {
                days.Add(day);
            }
            
            WeeklyDays = string.Join(",", days);
            UpdateWeekDaysSelected();
        }
        
        private void UpdateWeekDaysSelected()
        {
            if (string.IsNullOrWhiteSpace(WeeklyDays))
            {
                WeekDaysSelected = "No days selected";
                return;
            }
            
            var days = WeeklyDays.Split(',')
                .Select(d => d.Trim())
                .Where(d => !string.IsNullOrEmpty(d))
                .ToList();
                
            if (days.Count == 0)
            {
                WeekDaysSelected = "No days selected";
            }
            else if (days.Count == 7)
            {
                WeekDaysSelected = "Every day";
            }
            else
            {
                WeekDaysSelected = string.Join(", ", days);
            }
        }
    }
}