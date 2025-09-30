using HabitTrackerApp.MAUI.ViewModels;

namespace HabitTrackerApp.MAUI.Views;

public partial class HabitDetailPage : ContentPage
{
    public HabitDetailPage(HabitDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}