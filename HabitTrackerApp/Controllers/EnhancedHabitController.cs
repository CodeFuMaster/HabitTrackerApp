using Microsoft.AspNetCore.Mvc;
using HabitTrackerApp.Core.Data;
using HabitTrackerApp.Core.Services;
using HabitTrackerApp.Core.Models;
using HabitTrackerApp.Core.Models.Enhanced;

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

    public EnhancedHabitController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
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
}