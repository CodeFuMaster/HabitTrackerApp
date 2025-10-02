using Microsoft.AspNetCore.SignalR.Client;
using HabitTrackerApp.Core.Services.Sync;
using System.Text.Json;

namespace HabitTrackerApp.MAUI.Services
{
    /// <summary>
    /// SignalR service for real-time synchronization with the separate API server
    /// </summary>
    public class SignalRSyncService : IDisposable
    {
        private HubConnection? _hubConnection;
        private readonly string _deviceId;
        private readonly HttpClient _httpClient;
        private string? _serverUrl;
        private bool _isConnected;

        public event Action<bool>? ConnectionStatusChanged;
        public event Action<SyncRecord>? DataChanged;
        public event Action<string>? SyncCompleted;
        public event Action<string>? DeviceConnected;
        public event Action<string>? DeviceDisconnected;

        public bool IsConnected => _isConnected;
        public string DeviceId => _deviceId;

        public SignalRSyncService()
        {
            _deviceId = GetOrCreateDeviceId();
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        /// <summary>
        /// Discover and connect to the sync server on local network
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (_serverUrl == null)
                {
                    _serverUrl = await DiscoverServerAsync();
                    if (_serverUrl == null)
                    {
                        return false;
                    }
                }

                if (_hubConnection == null)
                {
                    _hubConnection = new HubConnectionBuilder()
                        .WithUrl($"{_serverUrl}/sync-hub")
                        .WithAutomaticReconnect()
                        .Build();

                    SetupEventHandlers();
                }

                if (_hubConnection.State == HubConnectionState.Disconnected)
                {
                    await _hubConnection.StartAsync();
                    await _hubConnection.InvokeAsync("JoinSyncGroup", _deviceId);

                    _isConnected = true;
                    ConnectionStatusChanged?.Invoke(true);
                    return true;
                }

                return _hubConnection.State == HubConnectionState.Connected;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SignalR connection failed: {ex.Message}");
                _isConnected = false;
                ConnectionStatusChanged?.Invoke(false);
                return false;
            }
        }

        /// <summary>
        /// Disconnect from the sync server
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                if (_hubConnection != null)
                {
                    await _hubConnection.StopAsync();
                }
                _isConnected = false;
                ConnectionStatusChanged?.Invoke(false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SignalR disconnection error: {ex.Message}");
            }
        }

        /// <summary>
        /// Notify other devices about data changes
        /// </summary>
        public async Task NotifyDataChangedAsync(string tableName, int recordId, string operation, object? data = null)
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("NotifyDataChanged", tableName, recordId, operation, data);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to notify data change: {ex.Message}");
            }
        }

        /// <summary>
        /// Request immediate sync from all connected devices
        /// </summary>
        public async Task RequestSyncAsync()
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("RequestSync");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to request sync: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if sync server is available
        /// </summary>
        public async Task<bool> IsServerAvailableAsync()
        {
            try
            {
                if (_serverUrl == null)
                {
                    _serverUrl = await DiscoverServerAsync();
                }

                if (_serverUrl != null)
                {
                    var response = await _httpClient.GetAsync($"{_serverUrl}/api/sync/ping");
                    return response.IsSuccessStatusCode;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get sync statistics from server
        /// </summary>
        public async Task<object?> GetSyncStatsAsync()
        {
            try
            {
                if (_serverUrl != null)
                {
                    var response = await _httpClient.GetAsync($"{_serverUrl}/api/sync/stats");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        return JsonSerializer.Deserialize<object>(json);
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private void SetupEventHandlers()
        {
            if (_hubConnection == null) return;

            _hubConnection.On<object>("DataChanged", (changeData) =>
            {
                try
                {
                    var jsonElement = (JsonElement)changeData;
                    var syncRecord = new SyncRecord
                    {
                        TableName = jsonElement.GetProperty("tableName").GetString() ?? "",
                        RecordId = jsonElement.GetProperty("recordId").GetInt32(),
                        Operation = jsonElement.GetProperty("operation").GetString() ?? "",
                        Data = jsonElement.GetProperty("data").ToString(),
                        DeviceId = jsonElement.GetProperty("deviceId").GetString() ?? "",
                        Timestamp = jsonElement.GetProperty("timestamp").GetDateTime()
                    };

                    DataChanged?.Invoke(syncRecord);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing data change: {ex.Message}");
                }
            });

            _hubConnection.On<object>("SyncCompleted", (syncResult) =>
            {
                try
                {
                    var jsonElement = (JsonElement)syncResult;
                    var message = jsonElement.GetProperty("message").GetString() ?? "Sync completed";
                    SyncCompleted?.Invoke(message);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing sync completed: {ex.Message}");
                }
            });

            _hubConnection.On<string>("DeviceConnected", (deviceId) =>
            {
                DeviceConnected?.Invoke(deviceId);
            });

            _hubConnection.On<string>("DeviceDisconnected", (deviceId) =>
            {
                DeviceDisconnected?.Invoke(deviceId);
            });

            _hubConnection.On<object>("SyncRequested", (requestData) =>
            {
                // Handle sync request from other devices
                // This could trigger a local sync operation
                System.Diagnostics.Debug.WriteLine("Sync requested by another device");
            });

            _hubConnection.Closed += async (error) =>
            {
                _isConnected = false;
                ConnectionStatusChanged?.Invoke(false);
                
                if (error != null)
                {
                    System.Diagnostics.Debug.WriteLine($"SignalR connection closed with error: {error.Message}");
                }

                // Attempt to reconnect after a delay
                await Task.Delay(5000);
                _ = Task.Run(ConnectAsync);
            };

            _hubConnection.Reconnected += (connectionId) =>
            {
                _isConnected = true;
                ConnectionStatusChanged?.Invoke(true);
                System.Diagnostics.Debug.WriteLine($"SignalR reconnected with ID: {connectionId}");
                return Task.CompletedTask;
            };
        }

        private async Task<string?> DiscoverServerAsync()
        {
            // Scan common local network ranges for HabitTracker server
            var ipRanges = new[] { "192.168.1.", "192.168.0.", "10.0.0.", "127.0.0." };
            var port = 5266;

            // First try localhost
            try
            {
                var response = await _httpClient.GetAsync($"http://localhost:{port}/health");
                if (response.IsSuccessStatusCode)
                {
                    return $"http://localhost:{port}";
                }
            }
            catch { }

            // Then scan network
            foreach (var baseIp in ipRanges)
            {
                for (int i = 1; i < 255; i++)
                {
                    try
                    {
                        var testUrl = $"http://{baseIp}{i}:{port}";
                        var response = await _httpClient.GetAsync($"{testUrl}/health");
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

        private string GetOrCreateDeviceId()
        {
            try
            {
                var deviceIdFile = Path.Combine(FileSystem.AppDataDirectory, "device-id.txt");
                
                if (File.Exists(deviceIdFile))
                {
                    return File.ReadAllText(deviceIdFile);
                }
                
                var newDeviceId = $"MAUI-{Environment.MachineName}-{Guid.NewGuid():N}";
                File.WriteAllText(deviceIdFile, newDeviceId);
                return newDeviceId;
            }
            catch
            {
                return $"MAUI-{Environment.MachineName}-{Guid.NewGuid():N}";
            }
        }

        public void Dispose()
        {
            _hubConnection?.DisposeAsync();
            _httpClient?.Dispose();
        }
    }
}