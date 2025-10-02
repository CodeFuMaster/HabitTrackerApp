using System.Net.Http.Json;
using System.Text.Json;
using HabitTrackerApp.Core.Models;
using HabitTrackerApp.Core.Services.Sync;

namespace HabitTrackerApp.MAUI.Services
{
    /// <summary>
    /// Enhanced API service that connects to the separate sync API server
    /// </summary>
    public class EnhancedApiService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly SignalRSyncService _signalRService;
        private string? _serverUrl;

        public event Action<bool>? ServerAvailabilityChanged;
        public bool IsServerAvailable { get; private set; }

        public EnhancedApiService(SignalRSyncService signalRService)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            _signalRService = signalRService;

            // Subscribe to SignalR connection status
            _signalRService.ConnectionStatusChanged += OnConnectionStatusChanged;
        }

        /// <summary>
        /// Initialize connection to the sync server
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            var isAvailable = await _signalRService.IsServerAvailableAsync();
            if (isAvailable)
            {
                await _signalRService.ConnectAsync();
                _serverUrl = await DiscoverServerUrl();
                IsServerAvailable = true;
                ServerAvailabilityChanged?.Invoke(true);
                return true;
            }

            IsServerAvailable = false;
            ServerAvailabilityChanged?.Invoke(false);
            return false;
        }

        /// <summary>
        /// Get all habits from the server
        /// </summary>
        public async Task<List<Habit>?> GetHabitsAsync()
        {
            try
            {
                if (_serverUrl == null) return null;

                var response = await _httpClient.GetAsync($"{_serverUrl}/api/habits");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Habit>>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting habits: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Create a new habit
        /// </summary>
        public async Task<Habit?> CreateHabitAsync(Habit habit)
        {
            try
            {
                if (_serverUrl == null) return null;

                var response = await _httpClient.PostAsJsonAsync($"{_serverUrl}/api/habits", habit);
                if (response.IsSuccessStatusCode)
                {
                    var createdHabit = await response.Content.ReadFromJsonAsync<Habit>();
                    
                    // Notify other devices via SignalR
                    await _signalRService.NotifyDataChangedAsync("Habits", createdHabit?.Id ?? 0, "INSERT", createdHabit);
                    
                    return createdHabit;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating habit: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Update an existing habit
        /// </summary>
        public async Task<bool> UpdateHabitAsync(Habit habit)
        {
            try
            {
                if (_serverUrl == null) return false;

                var response = await _httpClient.PutAsJsonAsync($"{_serverUrl}/api/habits/{habit.Id}", habit);
                if (response.IsSuccessStatusCode)
                {
                    // Notify other devices via SignalR
                    await _signalRService.NotifyDataChangedAsync("Habits", habit.Id, "UPDATE", habit);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating habit: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Delete a habit
        /// </summary>
        public async Task<bool> DeleteHabitAsync(int habitId)
        {
            try
            {
                if (_serverUrl == null) return false;

                var response = await _httpClient.DeleteAsync($"{_serverUrl}/api/habits/{habitId}");
                if (response.IsSuccessStatusCode)
                {
                    // Notify other devices via SignalR
                    await _signalRService.NotifyDataChangedAsync("Habits", habitId, "DELETE");
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting habit: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Get daily habit entries for a specific date
        /// </summary>
        public async Task<List<DailyHabitEntry>?> GetDailyEntriesAsync(DateTime date)
        {
            try
            {
                if (_serverUrl == null) return null;

                var dateString = date.ToString("yyyy-MM-dd");
                var response = await _httpClient.GetAsync($"{_serverUrl}/api/daily-entries/{dateString}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<DailyHabitEntry>>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting daily entries: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Create or update a daily habit entry
        /// </summary>
        public async Task<DailyHabitEntry?> SaveDailyEntryAsync(DailyHabitEntry entry)
        {
            try
            {
                if (_serverUrl == null) return null;

                var response = await _httpClient.PostAsJsonAsync($"{_serverUrl}/api/daily-entries", entry);
                if (response.IsSuccessStatusCode)
                {
                    var savedEntry = await response.Content.ReadFromJsonAsync<DailyHabitEntry>();
                    
                    // Notify other devices via SignalR
                    await _signalRService.NotifyDataChangedAsync("DailyHabitEntries", savedEntry?.Id ?? 0, "UPSERT", savedEntry);
                    
                    return savedEntry;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving daily entry: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Trigger manual sync with the server
        /// </summary>
        public async Task<SyncResult?> TriggerSyncAsync()
        {
            try
            {
                if (_serverUrl == null) return null;

                var response = await _httpClient.PostAsync($"{_serverUrl}/api/sync/trigger-sync", null);
                if (response.IsSuccessStatusCode)
                {
                    var syncResult = await response.Content.ReadFromJsonAsync<SyncResult>();
                    
                    // Request sync from other connected devices
                    await _signalRService.RequestSyncAsync();
                    
                    return syncResult;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error triggering sync: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Get sync statistics from server
        /// </summary>
        public async Task<object?> GetSyncStatsAsync()
        {
            return await _signalRService.GetSyncStatsAsync();
        }

        /// <summary>
        /// Check server health
        /// </summary>
        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                if (_serverUrl == null) 
                {
                    _serverUrl = await DiscoverServerUrl();
                    if (_serverUrl == null) return false;
                }

                var response = await _httpClient.GetAsync($"{_serverUrl}/health");
                var isAvailable = response.IsSuccessStatusCode;
                
                if (IsServerAvailable != isAvailable)
                {
                    IsServerAvailable = isAvailable;
                    ServerAvailabilityChanged?.Invoke(isAvailable);
                }

                return isAvailable;
            }
            catch
            {
                if (IsServerAvailable)
                {
                    IsServerAvailable = false;
                    ServerAvailabilityChanged?.Invoke(false);
                }
                return false;
            }
        }

        private async Task<string?> DiscoverServerUrl()
        {
            var ipRanges = new[] { "127.0.0.", "192.168.1.", "192.168.0.", "10.0.0." };
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

        private void OnConnectionStatusChanged(bool isConnected)
        {
            if (IsServerAvailable != isConnected)
            {
                IsServerAvailable = isConnected;
                ServerAvailabilityChanged?.Invoke(isConnected);
            }
        }

        public void Dispose()
        {
            _signalRService.ConnectionStatusChanged -= OnConnectionStatusChanged;
            _httpClient?.Dispose();
        }
    }
}