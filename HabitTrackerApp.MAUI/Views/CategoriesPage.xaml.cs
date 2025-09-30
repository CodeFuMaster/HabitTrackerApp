using HabitTrackerApp.MAUI.ViewModels;

namespace HabitTrackerApp.MAUI.Views;

public partial class CategoriesPage : ContentPage
{
    public CategoriesPage(CategoriesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}