using HabitTrackerApp.MAUI.ViewModels;

namespace HabitTrackerApp.MAUI.Views;

public partial class HabitListPage : ContentPage
{
    public HabitListPage(HabitListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is HabitListViewModel viewModel)
        {
            await viewModel.LoadHabitsCommand.ExecuteAsync(null);
        }
    }
}