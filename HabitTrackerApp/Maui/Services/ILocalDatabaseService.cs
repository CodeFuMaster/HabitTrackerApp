using HabitTrackerMobile.Models;

namespace HabitTrackerMobile.Services;

/// <summary>
/// Interface for local SQLite database operations
/// </summary>
public interface ILocalDatabaseService
{
    Task InitializeAsync();
    
    // Habit operations
    Task<List<Habit>> GetHabitsAsync();
    Task<Habit?> GetHabitAsync(int id);
    Task<Habit> SaveHabitAsync(Habit habit);
    Task<bool> DeleteHabitAsync(int id);
    
    // Daily entries
    Task<List<DailyHabitEntry>> GetDailyEntriesAsync(DateTime date);
    Task<DailyHabitEntry> SaveDailyEntryAsync(DailyHabitEntry entry);
    
    // Routine sessions
    Task<List<RoutineSession>> GetRoutineSessionsAsync(int habitId);
    Task<RoutineSession?> GetRoutineSessionAsync(int id);
    Task<RoutineSession> SaveRoutineSessionAsync(RoutineSession session);
    Task<bool> DeleteRoutineSessionAsync(int id);
    
    // Categories
    Task<List<Category>> GetCategoriesAsync();
    Task<Category> SaveCategoryAsync(Category category);
    
    // Sync tracking
    Task MarkForSyncAsync<T>(T entity, string operation) where T : class;
    Task<List<SyncItem>> GetPendingSyncItemsAsync();
    Task ClearSyncItemAsync(int id);
}

/// <summary>
/// Represents an item that needs to be synchronized with the API
/// </summary>
public class SyncItem
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Operation { get; set; } = string.Empty; // CREATE, UPDATE, DELETE
    public string Data { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}