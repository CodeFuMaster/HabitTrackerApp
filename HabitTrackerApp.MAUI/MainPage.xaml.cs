namespace HabitTrackerApp.MAUI;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private async void OnViewHabitsClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//HabitListPage");
	}

	private async void OnAddHabitClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(Views.AddHabitPage));
	}

	private async void OnStatisticsClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//StatisticsPage");
	}

	private async void OnDailyViewClicked(object? sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//DailyViewPage");
	}
}
