using HabitTrackerApp.MAUI.ViewModels;

namespace HabitTrackerApp.MAUI.Views;

public partial class WeeklyViewPage : ContentPage
{
    public WeeklyViewPage(WeeklyViewViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}