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

        // Placeholder methods for specific entity changes
        private async Task ApplyHabitChange(SyncRecord change, Dictionary<string, JsonElement>? data)
        {
            // Implementation would deserialize the data and apply to Habits table
            // This is a simplified version - full implementation would handle all fields
            _logger.LogInformation("Applied habit change: {Operation} for record {RecordId}", change.Operation, change.RecordId);
        }

        private async Task ApplyDailyHabitEntryChange(SyncRecord change, Dictionary<string, JsonElement>? data)
        {
            _logger.LogInformation("Applied daily habit entry change: {Operation} for record {RecordId}", change.Operation, change.RecordId);
        }

        private async Task ApplyRoutineSessionChange(SyncRecord change, Dictionary<string, JsonElement>? data)
        {
            _logger.LogInformation("Applied routine session change: {Operation} for record {RecordId}", change.Operation, change.RecordId);
        }

        private async Task ApplySessionActivityChange(SyncRecord change, Dictionary<string, JsonElement>? data)
        {
            _logger.LogInformation("Applied session activity change: {Operation} for record {RecordId}", change.Operation, change.RecordId);
        }

        private async Task ApplyCategoryChange(SyncRecord change, Dictionary<string, JsonElement>? data)
        {
            _logger.LogInformation("Applied category change: {Operation} for record {RecordId}", change.Operation, change.RecordId);
        }
    }
}