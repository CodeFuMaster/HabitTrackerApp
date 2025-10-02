using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using HabitTrackerApp.Core.Services.Sync;
using HabitTrackerApp.SeparateAPI.Hubs;
using System.Text.Json;

namespace HabitTrackerApp.SeparateAPI.Controllers
{
    /// <summary>
    /// API controller for handling synchronization between devices
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SyncController : ControllerBase
    {
        private readonly IHubContext<SyncHub> _syncHubContext;
        private readonly ILogger<SyncController> _logger;
        
        // In a real implementation, this would be a proper database service
        private static readonly List<SyncRecord> _centralSyncLog = new();
        private static readonly Dictionary<string, DateTime> _deviceLastSync = new();

        public SyncController(IHubContext<SyncHub> syncHubContext, ILogger<SyncController> logger)
        {
            _syncHubContext = syncHubContext;
            _logger = logger;
        }

        /// <summary>
        /// Health check endpoint for server discovery
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { 
                Status = "Server available", 
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0"
            });
        }

        /// <summary>
        /// Receive changes from a device and broadcast to others
        /// </summary>
        [HttpPost("receive-changes")]
        public async Task<IActionResult> ReceiveChanges([FromBody] List<SyncRecord> changes)
        {
            try
            {
                _logger.LogInformation($"Received {changes.Count} sync changes");

                foreach (var change in changes)
                {
                    // Add to central sync log
                    change.Id = _centralSyncLog.Count + 1;
                    change.Synced = true; // Mark as synced since it reached the server
                    _centralSyncLog.Add(change);

                    // Broadcast change to all connected devices via SignalR
                    await _syncHubContext.Clients.Group("SyncGroup").SendAsync("DataChanged", new
                    {
                        TableName = change.TableName,
                        RecordId = change.RecordId,
                        Operation = change.Operation,
                        Data = change.Data,
                        DeviceId = change.DeviceId,
                        Timestamp = change.Timestamp
                    });
                }

                // Update device last sync time
                var deviceId = changes.FirstOrDefault()?.DeviceId;
                if (!string.IsNullOrEmpty(deviceId))
                {
                    _deviceLastSync[deviceId] = DateTime.UtcNow;
                }

                return Ok(new { 
                    Success = true, 
                    Message = $"Successfully processed {changes.Count} changes",
                    ProcessedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sync changes");
                return BadRequest(new { 
                    Success = false, 
                    Message = $"Error processing changes: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Get all changes since a specific timestamp for a device
        /// </summary>
        [HttpGet("changes-since/{timestamp}")]
        public async Task<ActionResult<List<SyncRecord>>> GetChangesSince(DateTime timestamp, [FromQuery] string? deviceId = null)
        {
            try
            {
                // Get changes since timestamp, excluding changes from the requesting device
                var changes = _centralSyncLog
                    .Where(c => c.Timestamp > timestamp && 
                               (string.IsNullOrEmpty(deviceId) || c.DeviceId != deviceId))
                    .OrderBy(c => c.Timestamp)
                    .ToList();

                _logger.LogInformation($"Returning {changes.Count} changes since {timestamp} for device {deviceId}");

                return Ok(changes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving changes since timestamp");
                return BadRequest(new { 
                    Success = false, 
                    Message = $"Error retrieving changes: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Get the latest sync status for all devices
        /// </summary>
        [HttpGet("sync-status")]
        public IActionResult GetSyncStatus()
        {
            var status = _deviceLastSync.Select(kvp => new
            {
                DeviceId = kvp.Key,
                LastSyncTime = kvp.Value,
                MinutesSinceSync = (int)(DateTime.UtcNow - kvp.Value).TotalMinutes
            }).ToList();

            return Ok(new
            {
                TotalDevices = status.Count,
                TotalSyncRecords = _centralSyncLog.Count,
                Devices = status,
                ServerTime = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Manually trigger sync request to all connected devices
        /// </summary>
        [HttpPost("trigger-sync")]
        public async Task<IActionResult> TriggerSync([FromQuery] string? requestedBy = "Server")
        {
            try
            {
                await _syncHubContext.Clients.Group("SyncGroup").SendAsync("SyncRequested", new
                {
                    RequestedBy = requestedBy,
                    Timestamp = DateTime.UtcNow
                });

                return Ok(new { 
                    Success = true, 
                    Message = "Sync request broadcasted to all connected devices" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering sync");
                return BadRequest(new { 
                    Success = false, 
                    Message = $"Error triggering sync: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Get sync statistics
        /// </summary>
        [HttpGet("stats")]
        public IActionResult GetSyncStats()
        {
            var stats = new
            {
                TotalSyncRecords = _centralSyncLog.Count,
                RecordsByOperation = _centralSyncLog.GroupBy(r => r.Operation)
                    .Select(g => new { Operation = g.Key, Count = g.Count() })
                    .ToList(),
                RecordsByTable = _centralSyncLog.GroupBy(r => r.TableName)
                    .Select(g => new { Table = g.Key, Count = g.Count() })
                    .ToList(),
                RecordsByDevice = _centralSyncLog.GroupBy(r => r.DeviceId)
                    .Select(g => new { DeviceId = g.Key, Count = g.Count() })
                    .ToList(),
                RecentChanges = _centralSyncLog
                    .OrderByDescending(r => r.Timestamp)
                    .Take(10)
                    .Select(r => new { 
                        r.TableName, 
                        r.Operation, 
                        r.DeviceId, 
                        r.Timestamp 
                    })
                    .ToList(),
                ConnectedDevices = _deviceLastSync.Count,
                ServerStartTime = DateTime.UtcNow // This would be stored properly in a real app
            };

            return Ok(stats);
        }

        /// <summary>
        /// Clear all sync data (for testing/reset purposes)
        /// </summary>
        [HttpDelete("clear-sync-data")]
        public IActionResult ClearSyncData()
        {
            _centralSyncLog.Clear();
            _deviceLastSync.Clear();
            
            _logger.LogWarning("All sync data cleared");
            
            return Ok(new { 
                Success = true, 
                Message = "All sync data cleared" 
            });
        }
    }
}