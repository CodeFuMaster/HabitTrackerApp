using SQLite;
using System.Text.Json;
using HabitTrackerMobile.Models;

namespace HabitTrackerMobile.Services;

/// <summary>
/// Local SQLite database service for offline functionality
/// </summary>
public class LocalDatabaseService : ILocalDatabaseService
{
    private readonly string _databasePath;
    private SQLiteAsyncConnection? _database;

    public LocalDatabaseService()
    {
        _databasePath = Path.Combine(FileSystem.AppDataDirectory, "HabitTracker.db");
    }

    public async Task InitializeAsync()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(_databasePath);
        
        // Create tables
        await _database.CreateTableAsync<Habit>();
        await _database.CreateTableAsync<Category>();
        await _database.CreateTableAsync<DailyHabitEntry>();
        await _database.CreateTableAsync<RoutineSession>();
        await _database.CreateTableAsync<SessionActivity>();
        await _database.CreateTableAsync<ActivityTemplate>();
        await _database.CreateTableAsync<ActivityMetric>();
        await _database.CreateTableAsync<SyncItem>();
    }

    private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database is null)
            await InitializeAsync();
        return _database!;
    }

    // Habit operations
    public async Task<List<Habit>> GetHabitsAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Habit>().ToListAsync();
    }

    public async Task<Habit?> GetHabitAsync(int id)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Habit>().Where(h => h.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Habit> SaveHabitAsync(Habit habit)
    {
        var db = await GetDatabaseAsync();
        
        if (habit.Id == 0)
        {
            await db.InsertAsync(habit);
            await MarkForSyncAsync(habit, "CREATE");
        }
        else
        {
            await db.UpdateAsync(habit);
            await MarkForSyncAsync(habit, "UPDATE");
        }
        
        return habit;
    }

    public async Task<bool> DeleteHabitAsync(int id)
    {
        var db = await GetDatabaseAsync();
        var habit = await GetHabitAsync(id);
        
        if (habit != null)
        {
            await db.DeleteAsync(habit);
            await MarkForSyncAsync(habit, "DELETE");
            return true;
        }
        
        return false;
    }

    // Daily entries
    public async Task<List<DailyHabitEntry>> GetDailyEntriesAsync(DateTime date)
    {
        var db = await GetDatabaseAsync();
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);
        
        return await db.Table<DailyHabitEntry>()
            .Where(e => e.Date >= startDate && e.Date < endDate)
            .ToListAsync();
    }

    public async Task<DailyHabitEntry> SaveDailyEntryAsync(DailyHabitEntry entry)
    {
        var db = await GetDatabaseAsync();
        
        if (entry.Id == 0)
        {
            await db.InsertAsync(entry);
            await MarkForSyncAsync(entry, "CREATE");
        }
        else
        {
            await db.UpdateAsync(entry);
            await MarkForSyncAsync(entry, "UPDATE");
        }
        
        return entry;
    }

    // Routine sessions
    public async Task<List<RoutineSession>> GetRoutineSessionsAsync(int habitId)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<RoutineSession>()
            .Where(rs => rs.HabitId == habitId)
            .OrderByDescending(rs => rs.StartTime)
            .ToListAsync();
    }

    public async Task<RoutineSession?> GetRoutineSessionAsync(int id)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<RoutineSession>().Where(rs => rs.Id == id).FirstOrDefaultAsync();
    }

    public async Task<RoutineSession> SaveRoutineSessionAsync(RoutineSession session)
    {
        var db = await GetDatabaseAsync();
        
        if (session.Id == 0)
        {
            await db.InsertAsync(session);
            await MarkForSyncAsync(session, "CREATE");
        }
        else
        {
            await db.UpdateAsync(session);
            await MarkForSyncAsync(session, "UPDATE");
        }
        
        // Save activities
        foreach (var activity in session.Activities)
        {
            activity.RoutineSessionId = session.Id;
            if (activity.Id == 0)
            {
                await db.InsertAsync(activity);
            }
            else
            {
                await db.UpdateAsync(activity);
            }
            
            // Save metrics
            foreach (var metric in activity.Metrics)
            {
                metric.SessionActivityId = activity.Id;
                if (metric.Id == 0)
                {
                    await db.InsertAsync(metric);
                }
                else
                {
                    await db.UpdateAsync(metric);
                }
            }
        }
        
        return session;
    }

    public async Task<bool> DeleteRoutineSessionAsync(int id)
    {
        var db = await GetDatabaseAsync();
        var session = await GetRoutineSessionAsync(id);
        
        if (session != null)
        {
            await db.DeleteAsync(session);
            await MarkForSyncAsync(session, "DELETE");
            return true;
        }
        
        return false;
    }

    // Categories
    public async Task<List<Category>> GetCategoriesAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Category>().ToListAsync();
    }

    public async Task<Category> SaveCategoryAsync(Category category)
    {
        var db = await GetDatabaseAsync();
        
        if (category.Id == 0)
        {
            await db.InsertAsync(category);
            await MarkForSyncAsync(category, "CREATE");
        }
        else
        {
            await db.UpdateAsync(category);
            await MarkForSyncAsync(category, "UPDATE");
        }
        
        return category;
    }

    // Sync tracking
    public async Task MarkForSyncAsync<T>(T entity, string operation) where T : class
    {
        var db = await GetDatabaseAsync();
        
        var syncItem = new SyncItem
        {
            EntityType = typeof(T).Name,
            EntityId = GetEntityId(entity),
            Operation = operation,
            Data = JsonSerializer.Serialize(entity),
            CreatedAt = DateTime.UtcNow
        };
        
        await db.InsertAsync(syncItem);
    }

    public async Task<List<SyncItem>> GetPendingSyncItemsAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<SyncItem>().OrderBy(s => s.CreatedAt).ToListAsync();
    }

    public async Task ClearSyncItemAsync(int id)
    {
        var db = await GetDatabaseAsync();
        await db.DeleteAsync<SyncItem>(id);
    }

    private int GetEntityId<T>(T entity) where T : class
    {
        var property = typeof(T).GetProperty("Id");
        if (property != null)
        {
            var value = property.GetValue(entity);
            if (value is int id)
                return id;
        }
        return 0;
    }
}