using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using HabitTrackerApp.API.Hubs;
using HabitTrackerApp.Core.Services.Sync;
using HabitTrackerApp.Core.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HabitTrackerApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SyncController : ControllerBase
    {
        private readonly HabitTrackerDbContext _context;
        private readonly IHubContext<SyncHub> _hubContext;
        private readonly ILogger<SyncController> _logger;

        public SyncController(
            HabitTrackerDbContext context,
            IHubContext<SyncHub> hubContext,
            ILogger<SyncController> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Health check endpoint to verify server availability
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { 
                status = "available", 
                timestamp = DateTime.UtcNow,
                server = "HabitTracker Local Sync Server",
                version = "1.0.0"
            });
        }

        /// <summary>
        /// Receive changes from client devices and apply them to the central database
        /// </summary>
        [HttpPost("receive-changes")]
        public async Task<IActionResult> ReceiveChanges([FromBody] List<SyncRecord> changes)
        {
            try
            {
                if (changes == null || !changes.Any())
                {
                    return Ok(new { success = true, message = "No changes to process", appliedChanges = 0 });
                }

                var appliedChanges = 0;
                var errors = new List<string>();

                using var transaction = await _context.Database.BeginTransactionAsync();

                foreach (var change in changes.OrderBy(c => c.Timestamp))
                {
                    try
                    {
                        await ApplyChangeToDatabase(change);
                        appliedChanges++;

                        // Broadcast change to other connected devices
                        await _hubContext.Clients.All.SendAsync("DataChanged", 
                            change.TableName, 
                            change.RecordId, 
                            change.Operation, 
                            change.DeviceId);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Failed to apply change {change.Id}: {ex.Message}");
                        _logger.LogError(ex, "Failed to apply sync change {ChangeId}", change.Id);
                    }
                }

                await transaction.CommitAsync();

                _logger.LogInformation("Applied {AppliedChanges} sync changes from device", appliedChanges);

                return Ok(new { 
                    success = true, 
                    appliedChanges, 
                    errors = errors.Any() ? errors : null 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process sync changes");
                return StatusCode(500, new { success = false, message = "Failed to process changes", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all changes from the server since a specific timestamp
        /// </summary>
        [HttpGet("changes-since/{timestamp}")]
        public async Task<ActionResult<List<SyncRecord>>> GetChangesSince(DateTime timestamp, [FromQuery] string? excludeDeviceId = null)
        {
            try
            {
                var query = _context.SyncLogs
                    .Where(sl => sl.Timestamp > timestamp);

                // Exclude changes from a specific device to avoid circular sync
                if (!string.IsNullOrEmpty(excludeDeviceId))
                {
                    query = query.Where(sl => sl.DeviceId != excludeDeviceId);
                }

                var changes = await query
                    .OrderBy(sl => sl.Timestamp)
                    .Take(1000) // Limit to prevent large responses
                    .ToListAsync();

                _logger.LogInformation("Retrieved {ChangeCount} changes since {Timestamp}", changes.Count, timestamp);

                return Ok(changes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve changes since {Timestamp}", timestamp);
                return StatusCode(500, new { error = "Failed to retrieve changes", message = ex.Message });
            }
        }

        /// <summary>
        /// Get sync status and statistics
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult> GetSyncStatus()
        {
            try
            {
                var totalRecords = await _context.SyncLogs.CountAsync();
                var recentChanges = await _context.SyncLogs
                    .Where(sl => sl.Timestamp > DateTime.UtcNow.AddHours(-24))
                    .CountAsync();

                var lastChange = await _context.SyncLogs
                    .OrderByDescending(sl => sl.Timestamp)
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    isOnline = true,
                    totalSyncRecords = totalRecords,
                    recentChanges24h = recentChanges,
                    lastChangeTimestamp = lastChange?.Timestamp,
                    serverTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get sync status");
                return StatusCode(500, new { error = "Failed to get sync status", message = ex.Message });
            }
        }

        /// <summary>
        /// Force cleanup of old sync records
        /// </summary>
        [HttpPost("cleanup")]
        public async Task<ActionResult> CleanupOldRecords([FromQuery] int daysToKeep = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                var oldRecords = await _context.SyncLogs
                    .Where(sl => sl.Timestamp < cutoffDate)
                    .ToListAsync();

                if (oldRecords.Any())
                {
                    _context.SyncLogs.RemoveRange(oldRecords);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Cleaned up {RecordCount} old sync records", oldRecords.Count);

                return Ok(new { 
                    success = true, 
                    cleanedRecords = oldRecords.Count,
                    cutoffDate 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup old sync records");
                return StatusCode(500, new { error = "Failed to cleanup records", message = ex.Message });
            }
        }

        /// <summary>
        /// Apply a single change to the database
        /// </summary>
        private async Task ApplyChangeToDatabase(SyncRecord change)
        {
            // Log the change to sync log for other devices
            var existingLog = await _context.SyncLogs
                .FirstOrDefaultAsync(sl => sl.TableName == change.TableName && 
                                          sl.RecordId == change.RecordId && 
                                          sl.DeviceId == change.DeviceId);

            if (existingLog == null)
            {
                _context.SyncLogs.Add(new SyncRecord
                {
                    TableName = change.TableName,
                    RecordId = change.RecordId,
                    Operation = change.Operation,
                    Timestamp = change.Timestamp,
                    Data = change.Data,
                    DeviceId = change.DeviceId,
                    Synced = true
                });
            }

            // Apply the actual change based on table name
            var changeData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(change.Data);
            
            switch (change.TableName.ToLowerInvariant())
            {
                case "habits":
                    await ApplyHabitChange(change, changeData);
                    break;
                case "dailyhabitentries":
                    await ApplyDailyHabitEntryChange(change, changeData);
                    break;
                case "routinesessions":
                    await ApplyRoutineSessionChange(change, changeData);
                    break;
                case "sessionactivities":
                    await ApplySessionActivityChange(change, changeData);
                    break;
                case "categories":
                    await ApplyCategoryChange(change, changeData);
                    break;
                default:
                    _logger.LogWarning("Unknown table name for sync: {TableName}", change.TableName);
                    break;
            }
        }

        // Entity-specific change application methods
        private async Task ApplyHabitChange(SyncRecord change, Dictionary<string, JsonElement>? data)
        {
            if (data == null) return;

            switch (change.Operation.ToUpperInvariant())
            {
                case "INSERT":
                case "UPDATE":
                    var habit = await _context.Habits.FindAsync(change.RecordId);
                    if (habit == null && change.Operation.ToUpperInvariant() == "INSERT")
                    {
                        habit = JsonSerializer.Deserialize<HabitTrackerApp.Core.Models.Habit>(change.Data);
                        if (habit != null)
                        {
                            _context.Habits.Add(habit);
                        }
                    }
                    else if (habit != null)
                    {
                        // Update existing habit with conflict detection
                        if (habit.LastModifiedDate < change.Timestamp)
                        {
                            UpdateHabitFromData(habit, data);
                            habit.LastModifiedDate = change.Timestamp;
                        }
                        else
                        {
                            _logger.LogWarning("Skipping habit update - local version is newer. HabitId: {HabitId}", change.RecordId);
                        }
                    }
                    break;

                case "DELETE":
                    var habitToDelete = await _context.Habits.FindAsync(change.RecordId);
                    if (habitToDelete != null)
                    {
                        habitToDelete.IsDeleted = true;
                        habitToDelete.LastModifiedDate = change.Timestamp;
                    }
                    break;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Applied habit change: {Operation} for record {RecordId}", change.Operation, change.RecordId);
        }

        private async Task ApplyDailyHabitEntryChange(SyncRecord change, Dictionary<string, JsonElement>? data)
        {
            if (data == null) return;

            switch (change.Operation.ToUpperInvariant())
            {
                case "INSERT":
                case "UPDATE":
                    var entry = await _context.DailyHabitEntries.FindAsync(change.RecordId);
                    if (entry == null && change.Operation.ToUpperInvariant() == "INSERT")
                    {
                        entry = JsonSerializer.Deserialize<HabitTrackerApp.Core.Models.DailyHabitEntry>(change.Data);
                        if (entry != null)
                        {
                            _context.DailyHabitEntries.Add(entry);
                        }
                    }
                    else if (entry != null)
                    {
                        UpdateDailyEntryFromData(entry, data);
                        entry.UpdatedAt = change.Timestamp;
                    }
                    break;

                case "DELETE":
                    var entryToDelete = await _context.DailyHabitEntries.FindAsync(change.RecordId);
                    if (entryToDelete != null)
                    {
                        _context.DailyHabitEntries.Remove(entryToDelete);
                    }
                    break;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Applied daily habit entry change: {Operation} for record {RecordId}", change.Operation, change.RecordId);
        }

        private async Task ApplyRoutineSessionChange(SyncRecord change, Dictionary<string, JsonElement>? data)
        {
            if (data == null) return;

            switch (change.Operation.ToUpperInvariant())
            {
                case "INSERT":
                case "UPDATE":
                    var session = await _context.RoutineSessions
                        .Include(rs => rs.Activities)
                        .FirstOrDefaultAsync(rs => rs.Id == change.RecordId);
                    
                    if (session == null && change.Operation.ToUpperInvariant() == "INSERT")
                    {
                        session = JsonSerializer.Deserialize<HabitTrackerApp.Core.Models.Enhanced.RoutineSession>(change.Data);
                        if (session != null)
                        {
                            _context.RoutineSessions.Add(session);
                        }
                    }
                    else if (session != null)
                    {
                        if (session.LastModifiedAt < change.Timestamp)
                        {
                            UpdateRoutineSessionFromData(session, data);
                            session.LastModifiedAt = change.Timestamp;
                        }
                    }
                    break;

                case "DELETE":
                    var sessionToDelete = await _context.RoutineSessions.FindAsync(change.RecordId);
                    if (sessionToDelete != null)
                    {
                        _context.RoutineSessions.Remove(sessionToDelete);
                    }
                    break;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Applied routine session change: {Operation} for record {RecordId}", change.Operation, change.RecordId);
        }

        private async Task ApplySessionActivityChange(SyncRecord change, Dictionary<string, JsonElement>? data)
        {
            if (data == null) return;

            switch (change.Operation.ToUpperInvariant())
            {
                case "INSERT":
                case "UPDATE":
                    var activity = await _context.SessionActivities
                        .Include(sa => sa.Metrics)
                        .FirstOrDefaultAsync(sa => sa.Id == change.RecordId);
                    
                    if (activity == null && change.Operation.ToUpperInvariant() == "INSERT")
                    {
                        activity = JsonSerializer.Deserialize<HabitTrackerApp.Core.Models.Enhanced.SessionActivity>(change.Data);
                        if (activity != null)
                        {
                            _context.SessionActivities.Add(activity);
                        }
                    }
                    else if (activity != null)
                    {
                        UpdateSessionActivityFromData(activity, data);
                    }
                    break;

                case "DELETE":
                    var activityToDelete = await _context.SessionActivities.FindAsync(change.RecordId);
                    if (activityToDelete != null)
                    {
                        _context.SessionActivities.Remove(activityToDelete);
                    }
                    break;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Applied session activity change: {Operation} for record {RecordId}", change.Operation, change.RecordId);
        }

        private async Task ApplyCategoryChange(SyncRecord change, Dictionary<string, JsonElement>? data)
        {
            if (data == null) return;

            switch (change.Operation.ToUpperInvariant())
            {
                case "INSERT":
                case "UPDATE":
                    var category = await _context.Categories.FindAsync(change.RecordId);
                    if (category == null && change.Operation.ToUpperInvariant() == "INSERT")
                    {
                        category = JsonSerializer.Deserialize<HabitTrackerApp.Core.Models.Category>(change.Data);
                        if (category != null)
                        {
                            _context.Categories.Add(category);
                        }
                    }
                    else if (category != null)
                    {
                        UpdateCategoryFromData(category, data);
                    }
                    break;

                case "DELETE":
                    var categoryToDelete = await _context.Categories.FindAsync(change.RecordId);
                    if (categoryToDelete != null)
                    {
                        _context.Categories.Remove(categoryToDelete);
                    }
                    break;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Applied category change: {Operation} for record {RecordId}", change.Operation, change.RecordId);
        }

        // Helper methods to update entities from JSON data
        private void UpdateHabitFromData(HabitTrackerApp.Core.Models.Habit habit, Dictionary<string, JsonElement> data)
        {
            if (data.TryGetValue("Name", out var name)) habit.Name = name.GetString() ?? habit.Name;
            if (data.TryGetValue("Description", out var desc)) habit.Description = desc.GetString();
            if (data.TryGetValue("ShortDescription", out var shortDesc)) habit.ShortDescription = shortDesc.GetString();
            if (data.TryGetValue("IsActive", out var isActive)) habit.IsActive = isActive.GetBoolean();
            if (data.TryGetValue("CategoryId", out var catId) && catId.ValueKind != JsonValueKind.Null) 
                habit.CategoryId = catId.GetInt32();
            if (data.TryGetValue("Tags", out var tags)) habit.Tags = tags.GetString();
            if (data.TryGetValue("ImageUrl", out var imgUrl)) habit.ImageUrl = imgUrl.GetString();
        }

        private void UpdateDailyEntryFromData(HabitTrackerApp.Core.Models.DailyHabitEntry entry, Dictionary<string, JsonElement> data)
        {
            if (data.TryGetValue("IsCompleted", out var isCompleted)) entry.IsCompleted = isCompleted.GetBoolean();
            if (data.TryGetValue("Reflection", out var reflection)) entry.Reflection = reflection.GetString();
            if (data.TryGetValue("Score", out var score) && score.ValueKind != JsonValueKind.Null) 
                entry.Score = score.GetInt32();
        }

        private void UpdateRoutineSessionFromData(HabitTrackerApp.Core.Models.Enhanced.RoutineSession session, Dictionary<string, JsonElement> data)
        {
            if (data.TryGetValue("IsCompleted", out var isCompleted)) session.IsCompleted = isCompleted.GetBoolean();
            if (data.TryGetValue("Notes", out var notes)) session.Notes = notes.GetString();
            if (data.TryGetValue("Rating", out var rating) && rating.ValueKind != JsonValueKind.Null) 
                session.Rating = rating.GetInt32();
            if (data.TryGetValue("StartedAt", out var startedAt) && startedAt.ValueKind != JsonValueKind.Null)
                session.StartedAt = startedAt.GetDateTime();
            if (data.TryGetValue("CompletedAt", out var completedAt) && completedAt.ValueKind != JsonValueKind.Null)
                session.CompletedAt = completedAt.GetDateTime();
        }

        private void UpdateSessionActivityFromData(HabitTrackerApp.Core.Models.Enhanced.SessionActivity activity, Dictionary<string, JsonElement> data)
        {
            if (data.TryGetValue("IsCompleted", out var isCompleted)) activity.IsCompleted = isCompleted.GetBoolean();
            if (data.TryGetValue("Notes", out var notes)) activity.Notes = notes.GetString();
            if (data.TryGetValue("StartTime", out var startTime) && startTime.ValueKind != JsonValueKind.Null)
                activity.StartTime = startTime.GetDateTime();
            if (data.TryGetValue("EndTime", out var endTime) && endTime.ValueKind != JsonValueKind.Null)
                activity.EndTime = endTime.GetDateTime();
            if (data.TryGetValue("PlannedDuration", out var duration) && duration.ValueKind != JsonValueKind.Null)
            {
                var durationStr = duration.GetString();
                if (!string.IsNullOrEmpty(durationStr) && TimeSpan.TryParse(durationStr, out var ts))
                    activity.PlannedDuration = ts;
            }
        }

        private void UpdateCategoryFromData(HabitTrackerApp.Core.Models.Category category, Dictionary<string, JsonElement> data)
        {
            if (data.TryGetValue("Name", out var name)) category.Name = name.GetString() ?? category.Name;
            if (data.TryGetValue("Description", out var desc)) category.Description = desc.GetString();
            if (data.TryGetValue("ImageUrl", out var imgUrl)) category.ImageUrl = imgUrl.GetString();
        }
    }
}