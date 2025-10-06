using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HabitTrackerApp.Core.Data;
using HabitTrackerApp.Core.Services;
using HabitTrackerApp.Core.Models;
using HabitTrackerApp.Core.Models.Enhanced;
using HabitTrackerApp.Models;
using HabitTrackerApp.Data;

namespace HabitTrackerApp.Controllers;

/// <summary>
/// Enhanced Habit Controller with Phase 3 database functionality
/// Tests complex routine tracking and offline-first architecture
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EnhancedHabitController : ControllerBase
{
    private readonly DatabaseService _databaseService;
    private readonly AppDbContext _context;

    public EnhancedHabitController(DatabaseService databaseService, AppDbContext context)
    {
        _databaseService = databaseService;
        _context = context;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "ok", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Initialize database with sample data for testing
    /// </summary>
    [HttpPost("initialize")]
    public async Task<IActionResult> InitializeDatabase()
    {
        try
        {
            await _databaseService.InitializeDatabaseAsync();
            return Ok(new { message = "Database initialized successfully", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all habits with enhanced tracking capabilities
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHabits()
    {
        try
        {
            var habits = await _databaseService.GetHabitsAsync();
            return Ok(habits);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get detailed habit information including recent sessions
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetHabitDetail(int id)
    {
        try
        {
            var habit = await _databaseService.GetHabitByIdAsync(id);
            if (habit == null)
                return NotFound(new { message = "Habit not found" });

            var recentSessions = await _databaseService.GetRoutineSessionsAsync(id, DateTime.Today.AddDays(-7));
            
            return Ok(new
            {
                habit,
                recentSessions,
                totalSessions = recentSessions.Count,
                completedSessions = recentSessions.Count(s => s.IsCompleted)
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Test Tuesday Gym scenario - Create a complete workout session
    /// </summary>
    [HttpPost("{id}/test-gym-session")]
    public async Task<IActionResult> TestGymSession(int id)
    {
        try
        {
            var habit = await _databaseService.GetHabitByIdAsync(id);
            if (habit == null || !habit.Name.Contains("Gym"))
                return BadRequest(new { message = "This is not a gym habit" });

            // Create Tuesday Gym session
            var session = new RoutineSession
            {
                HabitId = id,
                Date = DateTime.Today,
                Name = "Tuesday Gym Session",
                StartTime = DateTime.Now,
                IsCompleted = false
            };

            // Add gym activities
            session.Activities = new List<SessionActivity>
            {
                new()
                {
                    Name = "Trap-bar Deadlift 5×3",
                    Type = ActivityType.Strength,
                    StartTime = DateTime.Now,
                    Order = 1,
                    IsCompleted = false,
                    Metrics = new List<ActivityMetric>
                    {
                        new() { Name = "Sets", DataType = MetricDataType.Numeric, NumericValue = 5 },
                        new() { Name = "Reps", DataType = MetricDataType.Numeric, NumericValue = 3 },
                        new() { Name = "Weight", DataType = MetricDataType.Numeric, NumericValue = 100, Unit = "kg" }
                    }
                },
                new()
                {
                    Name = "Pull-ups 4×8",
                    Type = ActivityType.Strength,
                    StartTime = DateTime.Now.AddMinutes(10),
                    Order = 2,
                    IsCompleted = false,
                    Metrics = new List<ActivityMetric>
                    {
                        new() { Name = "Sets", DataType = MetricDataType.Numeric, NumericValue = 4 },
                        new() { Name = "Reps", DataType = MetricDataType.Numeric, NumericValue = 8 }
                    }
                }
            };

            var savedSession = await _databaseService.SaveRoutineSessionAsync(session);

            // Complete the activities
            foreach (var activity in savedSession.Activities)
            {
                activity.IsCompleted = true;
                activity.EndTime = DateTime.Now.AddMinutes(30);
                await _databaseService.SaveSessionActivityAsync(activity);
            }

            // Complete the session
            savedSession.IsCompleted = true;
            savedSession.EndTime = DateTime.Now.AddMinutes(45);
            await _databaseService.SaveRoutineSessionAsync(savedSession);

            return Ok(new
            {
                message = "Gym session completed successfully",
                session = savedSession,
                duration = savedSession.Duration?.TotalMinutes,
                activitiesCompleted = savedSession.Activities.Count
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Test Morning Routine scenario - Create a timed routine session
    /// </summary>
    [HttpPost("{id}/test-morning-routine")]
    public async Task<IActionResult> TestMorningRoutine(int id)
    {
        try
        {
            var habit = await _databaseService.GetHabitByIdAsync(id);
            if (habit == null || !habit.Name.Contains("Morning"))
                return BadRequest(new { message = "This is not a morning routine habit" });

            var session = new RoutineSession
            {
                HabitId = id,
                Date = DateTime.Today,
                Name = "Morning Routine",
                StartTime = DateTime.Now,
                IsCompleted = false
            };

            session.Activities = new List<SessionActivity>
            {
                new()
                {
                    Name = "Wim Hof Breathing",
                    Type = ActivityType.Breathing,
                    StartTime = DateTime.Now,
                    PlannedDuration = TimeSpan.FromMinutes(15),
                    Order = 1,
                    IsCompleted = false,
                    Metrics = new List<ActivityMetric>
                    {
                        new() { Name = "Rounds", DataType = MetricDataType.Numeric, NumericValue = 4 },
                        new() { Name = "Hold Time", DataType = MetricDataType.Time, TimeValue = TimeSpan.FromSeconds(90) }
                    }
                },
                new()
                {
                    Name = "Meditation",
                    Type = ActivityType.Meditation,
                    StartTime = DateTime.Now.AddMinutes(15),
                    PlannedDuration = TimeSpan.FromMinutes(10),
                    Order = 2,
                    IsCompleted = false
                },
                new()
                {
                    Name = "Cold Shower",
                    Type = ActivityType.Recovery,
                    StartTime = DateTime.Now.AddMinutes(25),
                    PlannedDuration = TimeSpan.FromMinutes(2),
                    Order = 3,
                    IsCompleted = false
                }
            };

            var savedSession = await _databaseService.SaveRoutineSessionAsync(session);

            // Simulate completing timed activities
            foreach (var activity in savedSession.Activities)
            {
                activity.IsCompleted = true;
                activity.EndTime = activity.StartTime.Add(activity.PlannedDuration ?? TimeSpan.FromMinutes(1));
                await _databaseService.SaveSessionActivityAsync(activity);
            }

            savedSession.IsCompleted = true;
            savedSession.EndTime = DateTime.Now.AddMinutes(30);
            await _databaseService.SaveRoutineSessionAsync(savedSession);

            return Ok(new
            {
                message = "Morning routine completed successfully",
                session = savedSession,
                duration = savedSession.Duration?.TotalMinutes,
                timedActivities = savedSession.Activities.Where(a => a.PlannedDuration.HasValue).ToList()
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get activity templates for a habit
    /// </summary>
    [HttpGet("{id}/templates")]
    public async Task<IActionResult> GetActivityTemplates(int id)
    {
        try
        {
            var templates = await _databaseService.GetActivityTemplatesAsync(id);
            return Ok(templates);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get sync status - check for unsynced records
    /// </summary>
    [HttpGet("sync-status")]
    public async Task<IActionResult> GetSyncStatus()
    {
        try
        {
            var unsyncedRecords = await _databaseService.GetUnsyncedRecordsAsync();
            
            return Ok(new
            {
                hasUnsyncedData = unsyncedRecords.Any(),
                unsyncedCount = unsyncedRecords.Count,
                lastSyncTime = DateTime.UtcNow, // Would come from sync service
                records = unsyncedRecords.Take(10) // Show first 10 for debugging
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Sync endpoint - accepts client changes and returns success response
    /// </summary>
    [HttpPost("sync")]
    public async Task<IActionResult> Sync([FromBody] ClientChanges changes)
    {
        try
        {
            // Log what we received
            Console.WriteLine($"Received sync request:");
            Console.WriteLine($"  DeviceId: {changes?.DeviceId ?? "null"}");
            Console.WriteLine($"  Habits: {changes?.Habits?.Count ?? 0}");
            Console.WriteLine($"  Entries: {changes?.Entries?.Count ?? 0}");
            Console.WriteLine($"  Categories: {changes?.Categories?.Count ?? 0}");

            // Process client changes
            if (changes == null)
            {
                return BadRequest(new { error = "No changes provided" });
            }

            // Save habits
            if (changes.Habits != null && changes.Habits.Count > 0)
            {
                Console.WriteLine($"Saving {changes.Habits.Count} habits...");
                foreach (var habit in changes.Habits)
                {
                    await _databaseService.SaveHabitAsync(habit);
                }
            }

            // Save daily entries
            if (changes.Entries != null && changes.Entries.Count > 0)
            {
                Console.WriteLine($"Saving {changes.Entries.Count} entries...");
                foreach (var entry in changes.Entries)
                {
                    await _databaseService.SaveDailyEntryAsync(entry);
                }
            }

            // Save categories
            if (changes.Categories != null && changes.Categories.Count > 0)
            {
                Console.WriteLine($"Saving {changes.Categories.Count} categories...");
                foreach (var category in changes.Categories)
                {
                    await _databaseService.SaveCategoryAsync(category);
                }
            }

            // Save exercises
            if (changes.Exercises != null && changes.Exercises.Count > 0)
            {
                Console.WriteLine($"Saving {changes.Exercises.Count} exercises...");
                foreach (var exercise in changes.Exercises)
                {
                    if (exercise.Id == 0)
                    {
                        _context.Exercises.Add(exercise);
                    }
                    else
                    {
                        var existing = await _context.Exercises.FindAsync(exercise.Id);
                        if (existing != null)
                        {
                            _context.Entry(existing).CurrentValues.SetValues(exercise);
                        }
                        else
                        {
                            _context.Exercises.Add(exercise);
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }

            // Save exercise logs
            if (changes.ExerciseLogs != null && changes.ExerciseLogs.Count > 0)
            {
                Console.WriteLine($"Saving {changes.ExerciseLogs.Count} exercise logs...");
                foreach (var log in changes.ExerciseLogs)
                {
                    if (log.Id == 0)
                    {
                        _context.ExerciseLogs.Add(log);
                    }
                    else
                    {
                        var existing = await _context.ExerciseLogs.FindAsync(log.Id);
                        if (existing != null)
                        {
                            _context.Entry(existing).CurrentValues.SetValues(log);
                        }
                        else
                        {
                            _context.ExerciseLogs.Add(log);
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }

            Console.WriteLine("Sync completed successfully");

            // Return success response
            return Ok(new SyncResponse
            {
                Success = true,
                Conflicts = new List<object>(),
                ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sync error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { error = ex.Message, details = ex.ToString() });
        }
    }

    /// <summary>
    /// Get changes since a specific timestamp
    /// </summary>
    [HttpGet("changes-since")]
    public async Task<IActionResult> GetChangesSince([FromQuery] long timestamp, [FromQuery] string deviceId)
    {
        try
        {
            // Get all data (simplified - in production would filter by timestamp)
            var habits = await _databaseService.GetHabitsAsync();
            var categories = await _databaseService.GetCategoriesAsync();
            var exercises = await _context.Exercises.Where(e => e.IsActive).ToListAsync();
            var exerciseLogs = await _context.ExerciseLogs.ToListAsync();

            return Ok(new SyncChanges
            {
                Habits = habits,
                Entries = new List<HabitTrackerApp.Core.Models.DailyHabitEntry>(),
                Categories = categories,
                RoutineSessions = new List<RoutineSession>(),
                Exercises = exercises,
                ExerciseLogs = exerciseLogs,
                ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

// DTOs for sync - using Core models for compatibility
public class ClientChanges
{
    public string DeviceId { get; set; } = string.Empty;
    public long LastSyncTimestamp { get; set; }
    public List<HabitTrackerApp.Core.Models.Habit> Habits { get; set; } = new();
    public List<HabitTrackerApp.Core.Models.DailyHabitEntry> Entries { get; set; } = new();
    public List<HabitTrackerApp.Core.Models.Category> Categories { get; set; } = new();
    public List<RoutineSession> RoutineSessions { get; set; } = new();
    public List<HabitTrackerApp.Models.Exercise> Exercises { get; set; } = new();
    public List<HabitTrackerApp.Models.ExerciseLog> ExerciseLogs { get; set; } = new();
}

public class SyncResponse
{
    public bool Success { get; set; }
    public List<object> Conflicts { get; set; } = new();
    public long ServerTimestamp { get; set; }
}

public class SyncChanges
{
    public List<HabitTrackerApp.Core.Models.Habit> Habits { get; set; } = new();
    public List<HabitTrackerApp.Core.Models.DailyHabitEntry> Entries { get; set; } = new();
    public List<HabitTrackerApp.Core.Models.Category> Categories { get; set; } = new();
    public List<RoutineSession> RoutineSessions { get; set; } = new();
    public List<HabitTrackerApp.Models.Exercise> Exercises { get; set; } = new();
    public List<HabitTrackerApp.Models.ExerciseLog> ExerciseLogs { get; set; } = new();
    public long ServerTimestamp { get; set; }
}