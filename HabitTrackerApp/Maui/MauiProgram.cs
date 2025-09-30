using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using HabitTrackerMobile.Services;
using HabitTrackerMobile.ViewModels;

namespace HabitTrackerMobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Configure HTTP Client for API communication
		builder.Services.AddHttpClient("HabitTrackerAPI", client =>
		{
#if ANDROID
			// Use your computer's IP for Android devices to access local API
			client.BaseAddress = new Uri("http://192.168.1.103:5178/api/");
#else
			// Use localhost for other platforms
			client.BaseAddress = new Uri("http://localhost:5178/api/");
#endif
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		});

		// Register Services
		builder.Services.AddSingleton<IApiService, ApiService>();
		builder.Services.AddSingleton<ILocalDatabaseService, LocalDatabaseService>();
		builder.Services.AddSingleton<ISyncService, SyncService>();

		// Register ViewModels
		builder.Services.AddTransient<MainViewModel>();
		builder.Services.AddTransient<HabitListViewModel>();
		builder.Services.AddTransient<HabitDetailViewModel>();
		builder.Services.AddTransient<RoutineSessionViewModel>();

		// Register Views (will be added when we create the Views)
		// builder.Services.AddTransient<MainPage>();
		// builder.Services.AddTransient<HabitListPage>();
		// builder.Services.AddTransient<HabitDetailPage>();
		// builder.Services.AddTransient<RoutineSessionPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
