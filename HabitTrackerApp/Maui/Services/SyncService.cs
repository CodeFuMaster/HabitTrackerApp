using System.Text.Json;
using HabitTrackerMobile.Models;

namespace HabitTrackerMobile.Services;

/// <summary>
/// Synchronization service for offline-first functionality
/// </summary>
public class SyncService : ISyncService
{
    private readonly IApiService _apiService;
    private readonly ILocalDatabaseService _localDatabase;
    private bool _isSyncing = false;

    public event EventHandler<SyncStatusEventArgs>? SyncStatusChanged;

    public SyncService(IApiService apiService, ILocalDatabaseService localDatabase)
    {
        _apiService = apiService;
        _localDatabase = localDatabase;
    }

    public async Task<bool> IsOnlineAsync()
    {
        try
        {
            return await _apiService.IsApiAvailableAsync();
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SyncDataAsync()
    {
        if (_isSyncing)
            return false;

        _isSyncing = true;
        NotifySyncStatus(true, true, "Starting synchronization...");

        try
        {
            var isOnline = await IsOnlineAsync();
            
            if (!isOnline)
            {
                NotifySyncStatus(false, false, "Offline - using local data");
                return false;
            }

            // Push pending changes first
            var pushSuccess = await PushPendingChangesAsync();
            
            // Then pull latest data
            var pullSuccess = await PullLatestDataAsync();
            
            var success = pushSuccess && pullSuccess;
            NotifySyncStatus(true, false, success ? "Sync completed successfully" : "Sync completed with errors");
            
            return success;
        }
        catch (Exception ex)
        {
            NotifySyncStatus(true, false, "Sync failed", ex);
            return false;
        }
        finally
        {
            _isSyncing = false;
        }
    }

    public async Task<bool> PushPendingChangesAsync()
    {
        try
        {
            var pendingItems = await _localDatabase.GetPendingSyncItemsAsync();
            
            foreach (var item in pendingItems)
            {
                var success = await ProcessSyncItem(item);
                if (success)
                {
                    await _localDatabase.ClearSyncItemAsync(item.Id);
                }
            }
            
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error pushing changes: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> PullLatestDataAsync()
    {
        try
        {
            // Pull habits
            var remoteHabits = await _apiService.GetHabitsAsync();
            foreach (var habit in remoteHabits)
            {
                await _localDatabase.SaveHabitAsync(habit);
            }

            // Pull categories
            var remoteCategories = await _apiService.GetCategoriesAsync();
            foreach (var category in remoteCategories)
            {
                await _localDatabase.SaveCategoryAsync(category);
            }

            // Pull today's daily entries
            var today = DateTime.Today;
            var remoteDailyEntries = await _apiService.GetDailyEntriesAsync(today);
            foreach (var entry in remoteDailyEntries)
            {
                await _localDatabase.SaveDailyEntryAsync(entry);
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error pulling data: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> ProcessSyncItem(SyncItem item)
    {
        try
        {
            switch (item.EntityType)
            {
                case nameof(Habit):
                    return await ProcessHabitSync(item);
                case nameof(DailyHabitEntry):
                    return await ProcessDailyEntrySync(item);
                case nameof(RoutineSession):
                    return await ProcessRoutineSessionSync(item);
                case nameof(Category):
                    return await ProcessCategorySync(item);
                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error processing sync item {item.Id}: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> ProcessHabitSync(SyncItem item)
    {
        var habit = JsonSerializer.Deserialize<Habit>(item.Data);
        if (habit == null) return false;

        switch (item.Operation)
        {
            case "CREATE":
                await _apiService.CreateHabitAsync(habit);
                return true;
            case "UPDATE":
                await _apiService.UpdateHabitAsync(habit);
                return true;
            case "DELETE":
                return await _apiService.DeleteHabitAsync(habit.Id);
            default:
                return false;
        }
    }

    private async Task<bool> ProcessDailyEntrySync(SyncItem item)
    {
        var entry = JsonSerializer.Deserialize<DailyHabitEntry>(item.Data);
        if (entry == null) return false;

        switch (item.Operation)
        {
            case "CREATE":
                await _apiService.CreateDailyEntryAsync(entry);
                return true;
            case "UPDATE":
                await _apiService.UpdateDailyEntryAsync(entry);
                return true;
            default:
                return false;
        }
    }

    private async Task<bool> ProcessRoutineSessionSync(SyncItem item)
    {
        var session = JsonSerializer.Deserialize<RoutineSession>(item.Data);
        if (session == null) return false;

        switch (item.Operation)
        {
            case "CREATE":
                await _apiService.CreateRoutineSessionAsync(session);
                return true;
            case "UPDATE":
                await _apiService.UpdateRoutineSessionAsync(session);
                return true;
            case "DELETE":
                return await _apiService.DeleteRoutineSessionAsync(session.Id);
            default:
                return false;
        }
    }

    private async Task<bool> ProcessCategorySync(SyncItem item)
    {
        // Categories are typically read-only from API
        return true;
    }

    private void NotifySyncStatus(bool isOnline, bool isSyncing, string? message = null, Exception? error = null)
    {
        SyncStatusChanged?.Invoke(this, new SyncStatusEventArgs
        {
            IsOnline = isOnline,
            IsSyncing = isSyncing,
            Message = message,
            Error = error
        });
    }
}