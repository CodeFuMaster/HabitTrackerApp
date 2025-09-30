using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HabitTrackerApp.Core.Models;
using HabitTrackerApp.MAUI.Services;
using System.Collections.ObjectModel;

namespace HabitTrackerApp.MAUI.ViewModels;

public partial class CategoriesViewModel : ObservableObject
{
    private readonly IHabitService _habitService;

    // Parameterless constructor for XAML
    public CategoriesViewModel() : this(null!) { }

    public CategoriesViewModel(IHabitService habitService)
    {
        _habitService = habitService;
    }

    [ObservableProperty]
    private ObservableCollection<CategoryWithCount> categories = new();

    [ObservableProperty]
    private ObservableCollection<CategoryWithCount> filteredCategories = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool showAllCategories = true;

    [ObservableProperty]
    private bool showActiveOnly;

    [ObservableProperty]
    private CategoryWithCount? selectedCategory;

    partial void OnSearchTextChanged(string value)
    {
        FilterCategories();
    }

    partial void OnShowAllCategoriesChanged(bool value)
    {
        FilterCategories();
    }

    partial void OnShowActiveOnlyChanged(bool value)
    {
        FilterCategories();
    }

    [RelayCommand]
    private async Task LoadCategoriesAsync()
    {
        try
        {
            IsLoading = true;
            
            var allCategories = await _habitService.GetAllCategoriesAsync();
            var categoriesWithCount = new List<CategoryWithCount>();
            
            foreach (var category in allCategories)
            {
                var habits = new List<Habit>(); // await _habitService.GetHabitsByCategoryAsync(category.Id);
                var activeHabits = habits.Where(h => h.IsActive).ToList();
                
                categoriesWithCount.Add(new CategoryWithCount
                {
                    Category = category,
                    TotalHabits = habits.Count(),
                    ActiveHabits = activeHabits.Count,
                    CompletionRate = 0 // await CalculateCategoryCompletionRate(category.Id, activeHabits)
                });
            }
            
            Categories = new ObservableCollection<CategoryWithCount>(categoriesWithCount.OrderBy(c => c.Category.Name));
            FilterCategories();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading categories: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<double> CalculateCategoryCompletionRate(int categoryId, List<Habit> habits)
    {
        if (!habits.Any()) return 0;
        
        var totalPossible = 0;
        var totalCompleted = 0;
        var today = DateTime.Today;
        
        foreach (var habit in habits)
        {
            totalPossible++;
            var sessions = await _habitService.GetCompletedSessionsAsync(habit.Id, today);
            if (sessions.Any()) totalCompleted++;
        }
        
        return totalPossible > 0 ? (double)totalCompleted / totalPossible * 100 : 0;
    }

    private void FilterCategories()
    {
        var filtered = Categories.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(c => c.Category.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                          (c.Category.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        
        if (ShowActiveOnly)
        {
            filtered = filtered.Where(c => c.ActiveHabits > 0);
        }
        
        FilteredCategories = new ObservableCollection<CategoryWithCount>(filtered);
    }

    [RelayCommand]
    private async Task CreateCategoryAsync()
    {
        // This would open a dialog or navigate to create category page
        System.Diagnostics.Debug.WriteLine("Create category requested");
    }

    [RelayCommand]
    private async Task EditCategoryAsync(CategoryWithCount categoryWithCount)
    {
        if (categoryWithCount?.Category == null) return;
        
        // This would open edit dialog or navigate to edit page
        System.Diagnostics.Debug.WriteLine($"Edit category requested: {categoryWithCount.Category.Name}");
    }

    [RelayCommand]
    private async Task DeleteCategoryAsync(CategoryWithCount categoryWithCount)
    {
        if (categoryWithCount?.Category == null) return;
        
        try
        {
            // Check if category has habits
            var habits = new List<Habit>(); // await _habitService.GetHabitsByCategoryAsync(categoryWithCount.Category.Id);
            if (habits.Any())
            {
                // Show warning - category has habits
                System.Diagnostics.Debug.WriteLine($"Cannot delete category with habits: {categoryWithCount.Category.Name}");
                return;
            }
            
            await _habitService.DeleteCategoryAsync(categoryWithCount.Category.Id);
            await LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting category: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ViewCategoryDetailsAsync(CategoryWithCount categoryWithCount)
    {
        if (categoryWithCount?.Category == null) return;
        
        SelectedCategory = categoryWithCount;
        // This would navigate to category details page or show details view
        System.Diagnostics.Debug.WriteLine($"View category details: {categoryWithCount.Category.Name}");
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    [RelayCommand]
    private void ToggleView()
    {
        ShowAllCategories = !ShowAllCategories;
        ShowActiveOnly = !ShowActiveOnly;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadCategoriesAsync();
    }
}

public partial class CategoryWithCount : ObservableObject
{
    public Category Category { get; set; } = new() { Name = "Default" };
    
    [ObservableProperty]
    private int totalHabits;
    
    [ObservableProperty]
    private int activeHabits;
    
    [ObservableProperty]
    private double completionRate;
    
    [ObservableProperty]
    private bool isSelected;
    
    public string HabitCountText => $"{ActiveHabits} of {TotalHabits} habits";
    public string CompletionRateText => $"{CompletionRate:F1}% completed today";
}