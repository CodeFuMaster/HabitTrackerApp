using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HabitTrackerApp.Maui.Services
{
    public class NotificationRequest
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Category { get; set; } // e.g., "habit_reminder", "sync_complete"
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
        public DateTime? ScheduleTime { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }

    public interface INotificationService
    {
        Task<bool> Initialize();
        Task<bool> RequestPermission();
        Task<bool> SendLocalNotification(NotificationRequest request);
        Task<bool> ScheduleNotification(NotificationRequest request);
        Task<bool> CancelNotification(string id);
        Task<bool> CancelAllNotifications();
        Task<bool> RegisterForPushNotifications(string userId);
        Task<bool> UnregisterFromPushNotifications();
    }

    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IErrorHandlingService _errorHandlingService;
        private bool _isInitialized = false;

        public NotificationService(
            ILogger<NotificationService> logger,
            IErrorHandlingService errorHandlingService)
        {
            _logger = logger;
            _errorHandlingService = errorHandlingService;
        }

        public async Task<bool> Initialize()
        {
            if (_isInitialized)
                return true;

            try
            {
                _logger.LogInformation("Initializing notification service");

                // Request permissions early to improve UX
                await RequestPermission();

                // Handle incoming notifications (could be from a tap on notification)
                Microsoft.Maui.ApplicationModel.Platform.OnNotificationReceived(OnNotificationReceived);

                _isInitialized = true;
                _logger.LogInformation("Notification service initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing notification service");
                await _errorHandlingService.HandleExceptionAsync(ex, "Notification Initialization");
                return false;
            }
        }

        private void OnNotificationReceived(IDictionary<string, object> data)
        {
            try
            {
                _logger.LogInformation("Notification received with {Count} data items", data?.Count ?? 0);

                // Process notification data based on platform
                if (data != null)
                {
                    foreach (var key in data.Keys)
                    {
                        _logger.LogDebug("Notification data: {Key} = {Value}", key, data[key]);
                    }

                    // Handle different notification types
                    if (data.TryGetValue("type", out object type))
                    {
                        switch (type.ToString())
                        {
                            case "habit_reminder":
                                if (data.TryGetValue("habitId", out object habitId))
                                {
                                    _logger.LogInformation("Habit reminder notification for habit ID: {HabitId}", habitId);
                                    // Navigate to habit details or mark as completed
                                }
                                break;

                            case "sync_complete":
                                _logger.LogInformation("Sync complete notification");
                                // Maybe refresh data
                                break;

                            default:
                                _logger.LogInformation("Unknown notification type: {Type}", type);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification data");
            }
        }

        public async Task<bool> RequestPermission()
        {
            try
            {
                _logger.LogInformation("Requesting notification permission");

                var status = await Permissions.RequestAsync<Permissions.Notifications>();
                bool isGranted = status == PermissionStatus.Granted;

                _logger.LogInformation("Notification permission {Status}", isGranted ? "granted" : "denied");
                return isGranted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting notification permission");
                return false;
            }
        }

        public async Task<bool> SendLocalNotification(NotificationRequest request)
        {
            try
            {
                if (!_isInitialized)
                    await Initialize();

                _logger.LogInformation("Sending local notification: {Title}", request.Title);

                // Implement platform-specific notification logic
                // For this example, we'll use the MAUI implementation
                
                // Create the notification
                var notification = new LocalNotification
                {
                    Title = request.Title,
                    Body = request.Body,
                    Data = new Dictionary<string, string>(request.Data)
                };

                if (!string.IsNullOrEmpty(request.Category))
                {
                    notification.Data["type"] = request.Category;
                }

                if (!string.IsNullOrEmpty(request.Id))
                {
                    notification.Data["id"] = request.Id;
                }

                // Send the notification using MAUI API (this is simplified)
                await LocalNotificationCenter.Current.Show(notification);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending local notification");
                await _errorHandlingService.HandleExceptionAsync(ex, "Send Notification");
                return false;
            }
        }

        public async Task<bool> ScheduleNotification(NotificationRequest request)
        {
            try
            {
                if (!request.ScheduleTime.HasValue)
                {
                    _logger.LogWarning("Cannot schedule notification without a schedule time");
                    return false;
                }

                if (!_isInitialized)
                    await Initialize();

                _logger.LogInformation("Scheduling notification for {Time}: {Title}",
                    request.ScheduleTime.Value, request.Title);

                // Calculate delay until scheduled time
                var delay = request.ScheduleTime.Value - DateTime.Now;
                if (delay.TotalMilliseconds <= 0)
                {
                    _logger.LogWarning("Cannot schedule notification in the past");
                    return false;
                }

                // Create the notification
                var notification = new ScheduledNotification
                {
                    NotificationId = request.Id,
                    Title = request.Title,
                    Body = request.Body,
                    Data = new Dictionary<string, string>(request.Data),
                    Schedule = new NotificationSchedule
                    {
                        NotifyTime = request.ScheduleTime.Value
                    }
                };

                if (!string.IsNullOrEmpty(request.Category))
                {
                    notification.Data["type"] = request.Category;
                }

                // Schedule the notification using MAUI API (this is simplified)
                await LocalNotificationCenter.Current.Schedule(notification);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling notification");
                await _errorHandlingService.HandleExceptionAsync(ex, "Schedule Notification");
                return false;
            }
        }

        public async Task<bool> CancelNotification(string id)
        {
            try
            {
                if (!_isInitialized)
                    await Initialize();

                _logger.LogInformation("Canceling notification with ID: {Id}", id);

                // Cancel the notification using MAUI API (this is simplified)
                await LocalNotificationCenter.Current.Cancel(id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling notification");
                await _errorHandlingService.HandleExceptionAsync(ex, "Cancel Notification");
                return false;
            }
        }

        public async Task<bool> CancelAllNotifications()
        {
            try
            {
                if (!_isInitialized)
                    await Initialize();

                _logger.LogInformation("Canceling all notifications");

                // Cancel all notifications using MAUI API (this is simplified)
                await LocalNotificationCenter.Current.CancelAll();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling all notifications");
                await _errorHandlingService.HandleExceptionAsync(ex, "Cancel All Notifications");
                return false;
            }
        }

        public async Task<bool> RegisterForPushNotifications(string userId)
        {
            try
            {
                if (!_isInitialized)
                    await Initialize();

                _logger.LogInformation("Registering for push notifications. User ID: {UserId}", userId);

                // In a real implementation, this would:
                // 1. Get device token from Firebase (Android) or APNS (iOS)
                // 2. Register the token with your backend service

                // Mock implementation
                var token = Guid.NewGuid().ToString();
                _logger.LogInformation("Device token: {Token}", token);

                // This would be your API call to register the token
                await Task.Delay(500); // Simulate API call

                _logger.LogInformation("Successfully registered for push notifications");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering for push notifications");
                await _errorHandlingService.HandleExceptionAsync(ex, "Register Push Notifications");
                return false;
            }
        }

        public async Task<bool> UnregisterFromPushNotifications()
        {
            try
            {
                if (!_isInitialized)
                    await Initialize();

                _logger.LogInformation("Unregistering from push notifications");

                // In a real implementation, this would:
                // 1. Call your backend to unregister the device token

                // Mock implementation
                await Task.Delay(500); // Simulate API call

                _logger.LogInformation("Successfully unregistered from push notifications");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering from push notifications");
                await _errorHandlingService.HandleExceptionAsync(ex, "Unregister Push Notifications");
                return false;
            }
        }

        // This would be in a separate file in a real implementation
        // These are mock types to simulate the MAUI notification API
        private class LocalNotification
        {
            public string Title { get; set; }
            public string Body { get; set; }
            public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
        }

        private class ScheduledNotification : LocalNotification
        {
            public string NotificationId { get; set; }
            public NotificationSchedule Schedule { get; set; }
        }

        private class NotificationSchedule
        {
            public DateTime NotifyTime { get; set; }
        }

        // A mock static class to simulate MAUI's notification center
        private static class LocalNotificationCenter
        {
            public static LocalNotificationCenter Current { get; } = new LocalNotificationCenter();

            public async Task Show(LocalNotification notification)
            {
                await Task.Delay(100); // Simulate actual implementation
            }

            public async Task Schedule(ScheduledNotification notification)
            {
                await Task.Delay(100); // Simulate actual implementation
            }

            public async Task Cancel(string id)
            {
                await Task.Delay(100); // Simulate actual implementation
            }

            public async Task CancelAll()
            {
                await Task.Delay(100); // Simulate actual implementation
            }
        }
    }
}