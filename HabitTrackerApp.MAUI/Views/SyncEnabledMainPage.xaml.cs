using HabitTrackerApp.MAUI.ViewModels;

namespace HabitTrackerApp.MAUI.Views;

public partial class SyncEnabledMainPage : ContentPage
{
    private readonly SyncEnabledHabitListViewModel _viewModel;

    public SyncEnabledMainPage(SyncEnabledHabitListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Refresh data when page appears
        if (_viewModel.RefreshCommand.CanExecute(null))
        {
            _viewModel.RefreshCommand.Execute(null);
        }
    }

    private async void OnAddHabitClicked(object sender, EventArgs e)
    {
        var habitName = NewHabitEntry.Text?.Trim();
        if (!string.IsNullOrEmpty(habitName))
        {
            await _viewModel.AddHabitAsync(habitName);
            NewHabitEntry.Text = string.Empty;
        }
    }

    private async void OnViewAllHabitsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//habits");
    }

    private async void OnViewStatisticsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//statistics");
    }

    private async void OnWeeklyViewClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//weekly");
    }
}