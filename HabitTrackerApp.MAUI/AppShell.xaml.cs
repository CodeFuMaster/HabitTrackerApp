using HabitTrackerApp.MAUI.Views;

namespace HabitTrackerApp.MAUI;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Register routes for navigation
		Routing.RegisterRoute(nameof(HabitDetailPage), typeof(HabitDetailPage));
		Routing.RegisterRoute(nameof(AddHabitPage), typeof(AddHabitPage));
		Routing.RegisterRoute(nameof(HabitListPage), typeof(HabitListPage));
	}
}