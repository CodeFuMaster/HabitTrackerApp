using Microsoft.EntityFrameworkCore;
using HabitTrackerApp.Core.Data;
using HabitTrackerApp.Core.Models;
using HabitTrackerApp.Core.Models.Enhanced;
using System.Text.Json;

namespace HabitTrackerApp.Core.Services;

/// <summary>
/// Database service for offline-first SQLite operations
/// Handles CRUD operations with automatic sync tracking
/// </summary>
public class DatabaseService
{
    private readonly HabitTrackerDbContext _context;
    private readonly string _deviceId;
    
    public DatabaseService(HabitTrackerDbContext context)
    {
        _context = context;
        _deviceId = GetOrCreateDeviceId();
    }
    
    #region Habit Operations
    
    public async Task<List<Habit>> GetHabitsAsync()
    {
        return await _context.Habits
            .Include(h => h.Category)
            .Include(h => h.ActivityTemplates)
            .Where(h => h.IsActive)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }
    
    public async Task<Habit?> GetHabitByIdAsync(int id)
    {
        return await _context.Habits
            .Include(h => h.Category)
            .Include(h => h.ActivityTemplates)
            .Include(h => h.RoutineSessions)
                .ThenInclude(rs => rs.Activities)
                    .ThenInclude(sa => sa.Metrics)
            .FirstOrDefaultAsync(h => h.Id == id);
    }
    
    public async Task<Habit> SaveHabitAsync(Habit habit)
    {
        var isNew = habit.Id == 0;
        habit.UpdatedAt = DateTime.UtcNow;
        
        if (isNew)
        {
            habit.CreatedAt = DateTime.UtcNow;
            _context.Habits.Add(habit);
        }
        else
        {
            _context.Habits.Update(habit);
        }
        
        await _context.SaveChangesAsync();
        
        // Track for sync
        await TrackSyncRecord("Habits", habit.Id, isNew ? SyncOperation.Insert : SyncOperation.Update, habit);
        
        return habit;
    }
    
    #endregion
    
    #region Category Operations
    
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
    
    public async Task<Category> SaveCategoryAsync(Category category)
    {
        var isNew = category.Id == 0;
        category.UpdatedAt = DateTime.UtcNow;
        
        if (isNew)
        {
            category.CreatedAt = DateTime.UtcNow;
            _context.Categories.Add(category);
        }
        else
        {
            _context.Categories.Update(category);
        }
        
        await _context.SaveChangesAsync();
        
        // Track for sync
        await TrackSyncRecord("Categories", category.Id, isNew ? SyncOperation.Insert : SyncOperation.Update, category);
        
        return category;
    }
    
    #endregion
    
    #region Enhanced Routine Operations
    
    public async Task<RoutineSession> SaveRoutineSessionAsync(RoutineSession session)
    {
        var isNew = session.Id == 0;
        session.UpdatedAt = DateTime.UtcNow;
        
        if (isNew)
        {
            session.CreatedAt = DateTime.UtcNow;
            _context.RoutineSessions.Add(session);
        }
        else
        {
            _context.RoutineSessions.Update(session);
        }
        
        await _context.SaveChangesAsync();
        
        // Track for sync
        await TrackSyncRecord("RoutineSessions", session.Id, isNew ? SyncOperation.Insert : SyncOperation.Update, session);
        
        return session;
    }
    
    public async Task<SessionActivity> SaveSessionActivityAsync(SessionActivity activity)
    {
        var isNew = activity.Id == 0;
        activity.UpdatedAt = DateTime.UtcNow;
        
        if (isNew)
        {
            activity.CreatedAt = DateTime.UtcNow;
            _context.SessionActivities.Add(activity);
        }
        else
        {
            _context.SessionActivities.Update(activity);
        }
        
        await _context.SaveChangesAsync();
        
        // Track for sync
        await TrackSyncRecord("SessionActivities", activity.Id, isNew ? SyncOperation.Insert : SyncOperation.Update, activity);
        
        return activity;
    }
    
    public async Task<List<RoutineSession>> GetRoutineSessionsAsync(int habitId, DateTime? fromDate = null)
    {
        var query = _context.RoutineSessions
            .Include(rs => rs.Activities)
                .ThenInclude(sa => sa.Metrics)
            .Where(rs => rs.HabitId == habitId);
            
        if (fromDate.HasValue)
        {
            query = query.Where(rs => rs.Date >= fromDate.Value);
        }
        
        return await query
            .OrderByDescending(rs => rs.Date)
            .ThenByDescending(rs => rs.StartTime)
            .ToListAsync();
    }
    
    #endregion
    
    #region Activity Template Operations
    
    public async Task<List<ActivityTemplate>> GetActivityTemplatesAsync(int? habitId = null)
    {
        IQueryable<ActivityTemplate> query = _context.ActivityTemplates
            .Include(at => at.DefaultMetrics);
            
        if (habitId.HasValue)
        {
            query = query.Where(at => at.HabitId == habitId.Value || at.HabitId == null); // Include global templates
        }
        
        return await query
            .OrderBy(at => at.Name)
            .ToListAsync();
    }
    
    public async Task<ActivityTemplate> SaveActivityTemplateAsync(ActivityTemplate template)
    {
        var isNew = template.Id == 0;
        template.UpdatedAt = DateTime.UtcNow;
        
        if (isNew)
        {
            template.CreatedAt = DateTime.UtcNow;
            _context.ActivityTemplates.Add(template);
        }
        else
        {
            _context.ActivityTemplates.Update(template);
        }
        
        await _context.SaveChangesAsync();
        
        // Track for sync
        await TrackSyncRecord("ActivityTemplates", template.Id, isNew ? SyncOperation.Insert : SyncOperation.Update, template);
        
        return template;
    }
    
    #endregion
    
    #region Daily Entries (Legacy Support)
    
    public async Task<DailyHabitEntry?> GetDailyEntryAsync(int habitId, DateTime date)
    {
        return await _context.DailyHabitEntries
            .FirstOrDefaultAsync(e => e.HabitId == habitId && e.Date.Date == date.Date);
    }
    
    public async Task<DailyHabitEntry> SaveDailyEntryAsync(DailyHabitEntry entry)
    {
        var existing = await GetDailyEntryAsync(entry.HabitId, entry.Date);
        
        if (existing != null)
        {
            existing.IsCompleted = entry.IsCompleted;
            existing.Notes = entry.Notes;
            existing.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            await TrackSyncRecord("DailyHabitEntries", existing.Id, SyncOperation.Update, existing);
            return existing;
        }
        else
        {
            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;
            _context.DailyHabitEntries.Add(entry);
            
            await _context.SaveChangesAsync();
            await TrackSyncRecord("DailyHabitEntries", entry.Id, SyncOperation.Insert, entry);
            return entry;
        }
    }
    
    #endregion
    
    #region Sync Operations
    
    private async Task TrackSyncRecord<T>(string tableName, int recordId, SyncOperation operation, T data)
    {
        var syncRecord = new SyncRecord
        {
            TableName = tableName,
            RecordId = recordId,
            Operation = operation,
            Data = JsonSerializer.Serialize(data),
            DeviceId = _deviceId,
            Timestamp = DateTime.UtcNow,
            IsSynced = false
        };
        
        _context.SyncRecords.Add(syncRecord);
        await _context.SaveChangesAsync();
    }
    
    public async Task<List<SyncRecord>> GetUnsyncedRecordsAsync()
    {
        return await _context.SyncRecords
            .Where(sr => !sr.IsSynced)
            .OrderBy(sr => sr.Timestamp)
            .ToListAsync();
    }
    
    public async Task MarkRecordsSyncedAsync(List<int> syncRecordIds)
    {
        var records = await _context.SyncRecords
            .Where(sr => syncRecordIds.Contains(sr.Id))
            .ToListAsync();
            
        foreach (var record in records)
        {
            record.IsSynced = true;
        }
        
        await _context.SaveChangesAsync();
    }
    
    #endregion
    
    #region Device Management
    
    private string GetOrCreateDeviceId()
    {
        // This would be implemented to get a unique device identifier
        // For now, generate a simple one based on machine name and timestamp
        var machineName = Environment.MachineName;
        var deviceId = $"{machineName}_{DateTime.UtcNow.Ticks}";
        return deviceId;
    }
    
    public async Task RegisterDeviceAsync(string deviceName, string platform)
    {
        var existing = await _context.DeviceInfos
            .FirstOrDefaultAsync(d => d.DeviceId == _deviceId);
            
        if (existing == null)
        {
            var deviceInfo = new DeviceInfo
            {
                DeviceId = _deviceId,
                DeviceName = deviceName,
                Platform = platform,
                LastSyncTime = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _context.DeviceInfos.Add(deviceInfo);
            await _context.SaveChangesAsync();
        }
    }
    
    #endregion
    
    #region Database Initialization
    
    public async Task InitializeDatabaseAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        await SeedInitialDataAsync();
    }
    
    private async Task SeedInitialDataAsync()
    {
        // Check if data already exists
        if (await _context.Categories.AnyAsync()) return;
        
        // Seed categories based on your routine plan
        var categories = new List<Category>
        {
            new() { Name = "Fitness", Description = "Physical fitness and strength training" },
            new() { Name = "Wellness", Description = "Mental health and recovery" },
            new() { Name = "Martial Arts", Description = "Combat sports and self-defense" }
        };
        
        _context.Categories.AddRange(categories);
        await _context.SaveChangesAsync();
        
        // Seed default habits
        var gymCategory = categories.First(c => c.Name == "Fitness");
        var wellnessCategory = categories.First(c => c.Name == "Wellness");
        var martialArtsCategory = categories.First(c => c.Name == "Martial Arts");
        
        var habits = new List<Habit>
        {
            new()
            {
                Name = "Tuesday Gym",
                Description = "Strength training session with deadlifts, pull-ups, and accessory work",
                CategoryId = gymCategory.Id,
                UseEnhancedTracking = true,
                IsActive = true
            },
            new()
            {
                Name = "Morning Routine",
                Description = "Wim Hof breathing, meditation, cold shower, and manifestation",
                CategoryId = wellnessCategory.Id,
                UseEnhancedTracking = true,
                IsActive = true
            },
            new()
            {
                Name = "BJJ Training",
                Description = "Brazilian Jiu-Jitsu technique and sparring sessions",
                CategoryId = martialArtsCategory.Id,
                UseEnhancedTracking = true,
                IsActive = true
            },
            new()
            {
                Name = "Wing Chun Practice",
                Description = "Wing Chun forms, sticky hands, and solo training",
                CategoryId = martialArtsCategory.Id,
                UseEnhancedTracking = true,
                IsActive = true
            }
        };
        
        _context.Habits.AddRange(habits);
        await _context.SaveChangesAsync();
        
        // Seed activity templates
        await SeedActivityTemplatesAsync(habits);
    }
    
    private async Task SeedActivityTemplatesAsync(List<Habit> habits)
    {
        var gymHabit = habits.First(h => h.Name == "Tuesday Gym");
        var morningHabit = habits.First(h => h.Name == "Morning Routine");
        
        var templates = new List<ActivityTemplate>
        {
            // Gym templates
            new()
            {
                Name = "Trap-bar Deadlift",
                Type = ActivityType.Strength,
                Description = "Main compound movement for posterior chain",
                HabitId = gymHabit.Id,
                DefaultMetrics = new List<ActivityMetric>
                {
                    new() { Name = "Sets", DataType = MetricDataType.Numeric, NumericValue = 5 },
                    new() { Name = "Reps", DataType = MetricDataType.Numeric, NumericValue = 3 },
                    new() { Name = "Weight", DataType = MetricDataType.Numeric, Unit = "kg" }
                }
            },
            new()
            {
                Name = "Pull-ups",
                Type = ActivityType.Strength,
                Description = "Upper body pulling exercise",
                HabitId = gymHabit.Id,
                DefaultMetrics = new List<ActivityMetric>
                {
                    new() { Name = "Sets", DataType = MetricDataType.Numeric, NumericValue = 4 },
                    new() { Name = "Reps", DataType = MetricDataType.Numeric, NumericValue = 8 }
                }
            },
            
            // Morning routine templates
            new()
            {
                Name = "Wim Hof Breathing",
                Type = ActivityType.Breathing,
                Description = "Controlled breathing technique for energy and focus",
                HabitId = morningHabit.Id,
                DefaultDuration = TimeSpan.FromMinutes(15),
                DefaultMetrics = new List<ActivityMetric>
                {
                    new() { Name = "Rounds", DataType = MetricDataType.Numeric, NumericValue = 4 },
                    new() { Name = "Hold Time", DataType = MetricDataType.Time, TimeValue = TimeSpan.FromSeconds(90) }
                }
            },
            new()
            {
                Name = "Cold Shower",
                Type = ActivityType.Recovery,
                Description = "Cold exposure for recovery and mental resilience",
                HabitId = morningHabit.Id,
                DefaultDuration = TimeSpan.FromMinutes(2)
            }
        };
        
        _context.ActivityTemplates.AddRange(templates);
        await _context.SaveChangesAsync();
    }
    
    #endregion
}