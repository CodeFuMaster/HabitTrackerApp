using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.IO;
using HabitTrackerApp.Maui.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using HabitTrackerApp.Maui.Services.Repositories;

namespace HabitTrackerApp.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Configure Serilog
        SetupSerilog();
        
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
            
        // Register logging (with Serilog)
        builder.Logging.AddSerilog(dispose: true);

        // Register services for dependency injection
        RegisterServices(builder.Services);

        return builder.Build();
    }
    
    private static void SetupSerilog()
    {
        // Get the log file path in the app's data directory
        var logFolder = Path.Combine(FileSystem.AppDataDirectory, "Logs");
        Directory.CreateDirectory(logFolder);
        var logFile = Path.Combine(logFolder, "habittracker.log");
        
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.File(logFile, 
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7, // Keep a week of logs
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Debug()
            .CreateLogger();
    }
    
    private static void RegisterServices(IServiceCollection services)
    {
        // Register core services
        services.AddSingleton<IDatabaseService, SQLiteDatabaseService>();
        services.AddSingleton<IErrorHandlingService, ErrorHandlingService>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IConnectivityService, ConnectivityService>();
        services.AddSingleton<ISyncService, SyncService>();
        services.AddSingleton<INotificationService, NotificationService>();
        
        // Register localization services
        services.AddSingleton<IStringLocalizer, ResourceManagerStringLocalizer>();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        
        // Register repositories
        services.AddSingleton<IDailyHabitEntryRepository, DailyHabitEntryRepository>();
        services.AddSingleton<IDailyMetricValueRepository, DailyMetricValueRepository>();
        services.AddSingleton<IHabitMetricDefinitionRepository, HabitMetricDefinitionRepository>();
        services.AddSingleton<IHabitRepository, HabitRepository>();
        services.AddSingleton<ICategoryRepository, CategoryRepository>();
        
        // Register ViewModels (placeholder for the future implementation)
        // services.AddTransient<LoginViewModel>();
        // services.AddTransient<HabitsViewModel>();
    }
}
