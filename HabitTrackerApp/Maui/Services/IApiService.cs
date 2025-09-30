using HabitTrackerMobile.Models;

namespace HabitTrackerMobile.Services;

/// <summary>
/// Interface for API communication with the ASP.NET Core backend
/// </summary>
public interface IApiService
{
    // Habit operations
    Task<List<Habit>> GetHabitsAsync();
    Task<Habit?> GetHabitAsync(int id);
    Task<Habit> CreateHabitAsync(Habit habit);
    Task<Habit> UpdateHabitAsync(Habit habit);
    Task<bool> DeleteHabitAsync(int id);

    // Daily entries
    Task<List<DailyHabitEntry>> GetDailyEntriesAsync(DateTime date);
    Task<DailyHabitEntry> CreateDailyEntryAsync(DailyHabitEntry entry);
    Task<DailyHabitEntry> UpdateDailyEntryAsync(DailyHabitEntry entry);

    // Routine sessions
    Task<List<RoutineSession>> GetRoutineSessionsAsync(int habitId);
    Task<RoutineSession?> GetRoutineSessionAsync(int id);
    Task<RoutineSession> CreateRoutineSessionAsync(RoutineSession session);
    Task<RoutineSession> UpdateRoutineSessionAsync(RoutineSession session);
    Task<bool> DeleteRoutineSessionAsync(int id);

    // Categories
    Task<List<Category>> GetCategoriesAsync();

    // Activity templates
    Task<List<ActivityTemplate>> GetActivityTemplatesAsync(int? habitId = null);

    // Test endpoints
    Task<string> TestGymSessionAsync();
    Task<string> TestMorningRoutineAsync();

    // Connection test
    Task<bool> IsApiAvailableAsync();
}