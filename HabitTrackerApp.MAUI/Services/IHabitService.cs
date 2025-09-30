using HabitTrackerApp.Core.Models;
using HabitTrackerApp.Core.Models.Enhanced;

namespace HabitTrackerApp.MAUI.Services;

public interface IHabitService
{
    // Habit management
    Task<List<Habit>> GetAllHabitsAsync();
    Task<IEnumerable<Habit>> GetHabitsAsync();
    Task<List<Habit>> GetHabitsForDateAsync(DateTime date);
    Task<Habit?> GetHabitByIdAsync(int id);
    Task<Habit> CreateHabitAsync(Habit habit);
    Task<Habit> UpdateHabitAsync(Habit habit);
    Task DeleteHabitAsync(int id);
    
    // Session management
    Task<RoutineSession?> GetTodaysSessionAsync(int habitId);
    Task<RoutineSession?> GetActiveSessionAsync(int habitId);
    Task<RoutineSession> StartRoutineSessionAsync(int habitId);
    Task<RoutineSession> StartSessionAsync(RoutineSession session);
    Task CompleteRoutineSessionAsync(int sessionId);
    Task CompleteSessionAsync(int sessionId);
    Task<List<RoutineSession>> GetCompletedSessionsAsync(int habitId, DateTime date);
    Task<List<RoutineSession>> GetRecentSessionsAsync(int habitId, int count);
    
    // Activity management
    Task<SessionActivity> AddSessionActivityAsync(SessionActivity activity);
    Task UpdateSessionActivityAsync(SessionActivity activity);
    Task DeleteSessionActivityAsync(int activityId);
    
    // Category management
    Task<List<Category>> GetAllCategoriesAsync();
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(int categoryId);
}