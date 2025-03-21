using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HabitTrackerApp.Maui.Services
{
    public interface IConnectivityService
    {
        bool IsConnected { get; }
        Task<bool> CheckConnectivityAsync();
        event EventHandler<bool> ConnectivityChanged;
        void StartMonitoring();
        void StopMonitoring();
    }

    public class ConnectivityService : IConnectivityService
    {
        private readonly ILogger<ConnectivityService> _logger;
        private bool _isMonitoring = false;

        public bool IsConnected => Connectivity.NetworkAccess == NetworkAccess.Internet;

        public event EventHandler<bool> ConnectivityChanged;

        public ConnectivityService(ILogger<ConnectivityService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> CheckConnectivityAsync()
        {
            try
            {
                // First check the platform connectivity API
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    _logger.LogInformation("Device reports no internet connection");
                    return false;
                }

                // Then try to actually connect to a reliable endpoint to verify
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = await client.GetAsync("https://www.microsoft.com");
                    bool isConnected = response.IsSuccessStatusCode;
                    
                    _logger.LogInformation("Network connectivity check: {IsConnected}", isConnected);
                    return isConnected;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking connectivity");
                return false;
            }
        }

        public void StartMonitoring()
        {
            if (_isMonitoring)
                return;

            _logger.LogInformation("Starting connectivity monitoring");
            
            // Subscribe to the platform connectivity change event
            Connectivity.ConnectivityChanged += OnConnectivityChanged;
            _isMonitoring = true;
        }

        public void StopMonitoring()
        {
            if (!_isMonitoring)
                return;

            _logger.LogInformation("Stopping connectivity monitoring");
            
            // Unsubscribe from the platform connectivity change event
            Connectivity.ConnectivityChanged -= OnConnectivityChanged;
            _isMonitoring = false;
        }

        private void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            bool isConnected = e.NetworkAccess == NetworkAccess.Internet;
            _logger.LogInformation("Connectivity changed: {IsConnected}", isConnected);
            
            // Notify subscribers of the connectivity change
            ConnectivityChanged?.Invoke(this, isConnected);
        }
    }
}