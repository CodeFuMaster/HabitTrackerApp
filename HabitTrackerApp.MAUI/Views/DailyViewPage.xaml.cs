using HabitTrackerApp.MAUI.ViewModels;

namespace HabitTrackerApp.MAUI.Views;

public partial class DailyViewPage : ContentPage
{
    public DailyViewPage(DailyViewViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}