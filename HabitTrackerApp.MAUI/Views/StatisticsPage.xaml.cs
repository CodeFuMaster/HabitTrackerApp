using HabitTrackerApp.MAUI.ViewModels;

namespace HabitTrackerApp.MAUI.Views;

public partial class StatisticsPage : ContentPage
{
    public StatisticsPage(StatisticsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}