using Microsoft.Extensions.Logging;
using HabitTrackerApp.MAUI.Views;
using HabitTrackerApp.MAUI.ViewModels;
using HabitTrackerApp.MAUI.Services;
using HabitTrackerApp.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace HabitTrackerApp.MAUI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Database
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "habittracker.db");
		builder.Services.AddDbContext<HabitTrackerDbContext>(options =>
			options.UseSqlite($"Data Source={dbPath}"));

		// Services
		builder.Services.AddScoped<IHabitService, HabitService>();
		
		// Register App for dependency injection
		builder.Services.AddTransient<App>();

		// ViewModels
		builder.Services.AddTransient<HabitListViewModel>();
		builder.Services.AddTransient<HabitDetailViewModel>();
		builder.Services.AddTransient<DailyViewViewModel>();
		builder.Services.AddTransient<WeeklyViewViewModel>();
		builder.Services.AddTransient<CategoriesViewModel>();
		builder.Services.AddTransient<StatisticsViewModel>();

		// Views
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<HabitListPage>();
		builder.Services.AddTransient<HabitDetailPage>();
		builder.Services.AddTransient<AddHabitPage>();
		builder.Services.AddTransient<DailyViewPage>();
		builder.Services.AddTransient<WeeklyViewPage>();
		builder.Services.AddTransient<CategoriesPage>();
		builder.Services.AddTransient<StatisticsPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
