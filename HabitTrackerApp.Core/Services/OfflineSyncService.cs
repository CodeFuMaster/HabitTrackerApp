using HabitTrackerApp.Core.Interfaces;
using HabitTrackerApp.Core.Services.Sync;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.NetworkInformation;

namespace HabitTrackerApp.Core.Services
{
    /// <summary>
    /// Offline-first synchronization service implementation
    /// Handles local change tracking and server synchronization
    /// </summary>
    public class OfflineSyncService : ISyncService
    {
        private readonly string _databasePath;
        private readonly HttpClient _httpClient;
        private readonly string _deviceId;
        private string? _serverUrl;
        private Timer? _backgroundSyncTimer;
        
        public OfflineSyncService(string databasePath, string deviceId)
        {
            _databasePath = databasePath;
            _deviceId = deviceId;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(5); // Quick timeout for local network
        }

        public async Task LogChangeAsync(string tableName, int recordId, string operation, object data, string deviceId)
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO SyncLog (TableName, RecordId, Operation, Timestamp, Data, DeviceId, Synced)
                VALUES (@tableName, @recordId, @operation, @timestamp, @data, @deviceId, 0)";
            
            command.Parameters.AddWithValue("@tableName", tableName);
            command.Parameters.AddWithValue("@recordId", recordId);
            command.Parameters.AddWithValue("@operation", operation);
            command.Parameters.AddWithValue("@timestamp", DateTime.UtcNow);
            command.Parameters.AddWithValue("@data", JsonSerializer.Serialize(data));
            command.Parameters.AddWithValue("@deviceId", deviceId);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<SyncResult> SynchronizeAsync()
        {
            try
            {
                // Check if server is available
                if (!await IsServerAvailableAsync())
                {
                    return SyncResult.CreateFailure("Server not available for synchronization");
                }

                // Get pending changes
                var pendingChanges = await GetPendingChangesAsync();
                if (!pendingChanges.Any())
                {
                    return SyncResult.CreateSuccess(0, "No changes to synchronize");
                }

                // Send changes to server
                var response = await _httpClient.PostAsJsonAsync($"{_serverUrl}/api/sync/receive-changes", pendingChanges);
                
                if (response.IsSuccessStatusCode)
                {
                    // Mark changes as synced
                    await MarkChangesSyncedAsync(pendingChanges.Select(c => c.Id).ToList());
                    
                    // Get server changes since last sync
                    var lastSyncTime = await GetLastSyncTimeAsync();
                    var serverChangesResponse = await _httpClient.GetAsync($"{_serverUrl}/api/sync/changes-since/{lastSyncTime:O}");
                    
                    if (serverChangesResponse.IsSuccessStatusCode)
                    {
                        var serverChanges = await serverChangesResponse.Content.ReadFromJsonAsync<List<SyncRecord>>();
                        if (serverChanges?.Any() == true)
                        {
                            await ApplyServerChangesAsync(serverChanges);
                        }
                    }
                    
                    await UpdateLastSyncTimeAsync(DateTime.UtcNow);
                    return SyncResult.CreateSuccess(pendingChanges.Count, "Synchronization completed successfully");
                }
                else
                {
                    return SyncResult.CreateFailure($"Server responded with error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return SyncResult.CreateFailure($"Synchronization failed: {ex.Message}");
            }
        }

        public async Task<bool> IsServerAvailableAsync()
        {
            if (string.IsNullOrEmpty(_serverUrl))
            {
                _serverUrl = await DiscoverServerAsync();
                if (string.IsNullOrEmpty(_serverUrl))
                    return false;
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_serverUrl}/api/sync/ping");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string?> DiscoverServerAsync()
        {
            // Scan common local network ranges for HabitTracker server
            var ipRanges = new[] { "192.168.1.", "192.168.0.", "10.0.0.", "172.16.0." };
            var port = 5000;

            foreach (var baseIp in ipRanges)
            {
                for (int i = 1; i < 255; i++)
                {
                    var testUrl = $"http://{baseIp}{i}:{port}";
                    
                    try
                    {
                        var response = await _httpClient.GetAsync($"{testUrl}/api/sync/ping");
                        if (response.IsSuccessStatusCode)
                        {
                            return testUrl;
                        }
                    }
                    catch
                    {
                        // Ignore connection failures during discovery
                    }
                }
            }

            return null;
        }

        public async Task<List<SyncRecord>> GetPendingChangesAsync()
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Id, TableName, RecordId, Operation, Timestamp, Data, DeviceId, Synced
                FROM SyncLog 
                WHERE Synced = 0 
                ORDER BY Timestamp";

            var syncRecords = new List<SyncRecord>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                syncRecords.Add(new SyncRecord
                {
                    Id = reader.GetInt32(0), // Id
                    TableName = reader.GetString(1), // TableName
                    RecordId = reader.GetInt32(2), // RecordId
                    Operation = reader.GetString(3), // Operation
                    Timestamp = reader.GetDateTime(4), // Timestamp
                    Data = reader.GetString(5), // Data
                    DeviceId = reader.GetString(6), // DeviceId
                    Synced = reader.GetBoolean(7) // Synced
                });
            }

            return syncRecords;
        }

        public async Task<SyncResult> ApplyServerChangesAsync(List<SyncRecord> serverChanges)
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync();

            var appliedChanges = 0;
            var conflicts = new List<string>();

            foreach (var change in serverChanges.Where(c => c.DeviceId != _deviceId))
            {
                try
                {
                    // Apply change based on operation
                    switch (change.Operation.ToUpper())
                    {
                        case "INSERT":
                            await ApplyInsertChangeAsync(connection, change);
                            break;
                        case "UPDATE":
                            await ApplyUpdateChangeAsync(connection, change);
                            break;
                        case "DELETE":
                            await ApplyDeleteChangeAsync(connection, change);
                            break;
                    }
                    appliedChanges++;
                }
                catch (Exception ex)
                {
                    conflicts.Add($"Failed to apply {change.Operation} on {change.TableName}:{change.RecordId}: {ex.Message}");
                }
            }

            return new SyncResult
            {
                Success = true,
                Message = $"Applied {appliedChanges} server changes",
                SyncedRecords = appliedChanges,
                Conflicts = conflicts,
                SyncTimestamp = DateTime.UtcNow
            };
        }

        public async Task StartBackgroundSyncAsync()
        {
            // Start periodic sync every 30 seconds when server is available
            _backgroundSyncTimer = new Timer(async _ => await SynchronizeAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            await Task.CompletedTask;
        }

        public async Task StopBackgroundSyncAsync()
        {
            _backgroundSyncTimer?.Dispose();
            _backgroundSyncTimer = null;
            await Task.CompletedTask;
        }

        public async Task<SyncResult> ForceSyncAsync()
        {
            return await SynchronizeAsync();
        }

        // Private helper methods

        private async Task MarkChangesSyncedAsync(List<int> syncLogIds)
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = $"UPDATE SyncLog SET Synced = 1 WHERE Id IN ({string.Join(",", syncLogIds)})";
            await command.ExecuteNonQueryAsync();
        }

        private async Task<DateTime> GetLastSyncTimeAsync()
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Value FROM Settings WHERE Key = 'LastSyncTime'";
            
            var result = await command.ExecuteScalarAsync();
            if (result != null && DateTime.TryParse(result.ToString(), out var lastSync))
            {
                return lastSync;
            }

            return DateTime.MinValue;
        }

        private async Task UpdateLastSyncTimeAsync(DateTime syncTime)
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO Settings (Key, Value) 
                VALUES ('LastSyncTime', @syncTime)";
            command.Parameters.AddWithValue("@syncTime", syncTime.ToString("O"));
            await command.ExecuteNonQueryAsync();
        }

        private async Task ApplyInsertChangeAsync(SqliteConnection connection, SyncRecord change)
        {
            // Implementation would depend on table structure
            // This is a simplified example
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(change.Data);
            // Apply insert logic based on table name and data
        }

        private async Task ApplyUpdateChangeAsync(SqliteConnection connection, SyncRecord change)
        {
            // Implementation would depend on table structure
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(change.Data);
            // Apply update logic based on table name and data
        }

        private async Task ApplyDeleteChangeAsync(SqliteConnection connection, SyncRecord change)
        {
            // Apply delete logic based on table name and record ID
            var command = connection.CreateCommand();
            command.CommandText = $"DELETE FROM {change.TableName} WHERE Id = @recordId";
            command.Parameters.AddWithValue("@recordId", change.RecordId);
            await command.ExecuteNonQueryAsync();
        }

        public void Dispose()
        {
            _backgroundSyncTimer?.Dispose();
            _httpClient?.Dispose();
        }
    }
}