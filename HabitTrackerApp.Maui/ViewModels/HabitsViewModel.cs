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
    public partial class HabitsViewModel : BaseViewModel
    {
        private readonly IHabitRepository _habitRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IDailyHabitEntryRepository _entryRepository;
        
        [ObservableProperty]
        private ObservableCollection<Habit> _habits;
        
        [ObservableProperty]
        private ObservableCollection<Category> _categories;
        
        [ObservableProperty]
        private Habit _selectedHabit;
        
        [ObservableProperty]
        private Category _selectedCategory;
        
        [ObservableProperty]
        private string _searchText;
        
        [ObservableProperty]
        private DateTime _selectedDate;
        
        [ObservableProperty]
        private bool _showOnlyForToday;
        
        public HabitsViewModel(
            IHabitRepository habitRepository,
            ICategoryRepository categoryRepository,
            IDailyHabitEntryRepository entryRepository,
            IErrorHandlingService errorHandlingService,
            ILogger<HabitsViewModel> logger) 
            : base(errorHandlingService, logger)
        {
            _habitRepository = habitRepository;
            _categoryRepository = categoryRepository;
            _entryRepository = entryRepository;
            
            Title = "My Habits";
            SelectedDate = DateTime.Today;
            Habits = new ObservableCollection<Habit>();
            Categories = new ObservableCollection<Category>();
            ShowOnlyForToday = true;
        }
        
        [RelayCommand]
        public async Task LoadData()
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
                
                // Load habits
                await LoadHabits();
            }, "LoadDataOperation");
        }
        
        private async Task LoadHabits()
        {
            List<Habit> habits;
            
            if (ShowOnlyForToday)
            {
                // Get habits for selected date
                habits = await _habitRepository.GetHabitsForDateAsync(SelectedDate);
            }
            else if (SelectedCategory != null)
            {
                // Get habits for selected category
                habits = await _habitRepository.GetHabitsByCategoryAsync(SelectedCategory.Id);
            }
            else if (!string.IsNullOrWhiteSpace(SearchText))
            {
                // Search habits
                habits = await _habitRepository.SearchHabitsAsync(SearchText);
            }
            else
            {
                // Get all active habits
                habits = await _habitRepository.GetActiveHabitsAsync();
            }
            
            // Update UI collection
            Habits.Clear();
            foreach (var habit in habits)
            {
                Habits.Add(habit);
            }
            
            // Load habit entries for the selected date
            if (habits.Any())
            {
                var entries = await _entryRepository.GetEntriesForDateAsync(SelectedDate);
                
                // Update habit completion status
                foreach (var habit in Habits)
                {
                    var entry = entries.FirstOrDefault(e => e.HabitId == habit.Id);
                    if (entry != null)
                    {
                        habit.DailyHabitEntries.Add(entry);
                    }
                }
            }
        }
        
        [RelayCommand]
        private async Task ToggleHabitCompletion(Habit habit)
        {
            await SafeExecuteAsync(async () =>
            {
                if (habit == null)
                    return;
                
                Logger.LogInformation("Toggling completion for habit: {HabitId}, {HabitName}", habit.Id, habit.Name);
                
                // Get existing entry for this date and habit or create a new one
                var entry = await _entryRepository.GetEntryForHabitAndDateAsync(habit.Id, SelectedDate);
                if (entry == null)
                {
                    entry = new DailyHabitEntry
                    {
                        HabitId = habit.Id,
                        Date = SelectedDate,
                        IsCompleted = true, // Set to completed
                    };
                }
                else
                {
                    // Toggle completion status
                    entry.IsCompleted = !entry.IsCompleted;
                }
                
                // Save changes
                await _entryRepository.SaveAsync(entry);
                
                // Refresh the habit list to show updated status
                await LoadHabits();
                
                Logger.LogInformation("Habit completion toggled: {HabitId}, {IsCompleted}", 
                    habit.Id, entry.IsCompleted);
            }, "ToggleHabitCompletionOperation");
        }
        
        [RelayCommand]
        private async Task SelectDate(DateTime date)
        {
            SelectedDate = date;
            await LoadHabits();
        }
        
        [RelayCommand]
        private async Task FilterByCategory(Category category)
        {
            SelectedCategory = category;
            ShowOnlyForToday = false;
            await LoadHabits();
        }
        
        [RelayCommand]
        private async Task ClearFilter()
        {
            SelectedCategory = null;
            SearchText = string.Empty;
            ShowOnlyForToday = true;
            await LoadHabits();
        }
        
        [RelayCommand]
        private async Task Search()
        {
            ShowOnlyForToday = false;
            await LoadHabits();
        }
        
        [RelayCommand]
        private async Task SelectHabit(Habit habit)
        {
            if (habit == null)
                return;
            
            SelectedHabit = habit;
            
            // Navigate to habit details page
            var parameters = new Dictionary<string, object>
            {
                { "HabitId", habit.Id }
            };
            
            await Shell.Current.GoToAsync("HabitDetailsPage", parameters);
        }
        
        [RelayCommand]
        private async Task AddNewHabit()
        {
            await Shell.Current.GoToAsync("HabitEditPage");
        }
    }
}