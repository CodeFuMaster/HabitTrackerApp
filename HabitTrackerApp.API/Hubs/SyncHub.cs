using Microsoft.AspNetCore.SignalR;

namespace HabitTrackerApp.API.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time synchronization between devices
    /// Handles real-time notifications when habits, activities, or sessions are updated
    /// </summary>
    public class SyncHub : Hub
    {
        /// <summary>
        /// Join a sync group for a specific device
        /// This allows broadcasting updates to all devices except the sender
        /// </summary>
        public async Task JoinDeviceGroup(string deviceId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Device_{deviceId}");
            
            // Also join the global sync group for broadcast messages
            await Groups.AddToGroupAsync(Context.ConnectionId, "SyncGroup");
        }

        /// <summary>
        /// Leave a device group when disconnecting
        /// </summary>
        public async Task LeaveDeviceGroup(string deviceId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Device_{deviceId}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SyncGroup");
        }

        /// <summary>
        /// Notify other devices about data changes
        /// </summary>
        public async Task NotifyDataChanged(string tableName, int recordId, string operation, string deviceId)
        {
            // Broadcast to all devices except the sender
            await Clients.GroupExcept("SyncGroup", Context.ConnectionId).SendAsync(
                "DataChanged", 
                tableName, 
                recordId, 
                operation, 
                deviceId,
                DateTime.UtcNow
            );
        }

        /// <summary>
        /// Notify about habit completion status changes
        /// </summary>
        public async Task NotifyHabitCompletionChanged(int habitId, DateTime date, bool isCompleted, string deviceId)
        {
            await Clients.GroupExcept("SyncGroup", Context.ConnectionId).SendAsync(
                "HabitCompletionChanged",
                habitId,
                date,
                isCompleted,
                deviceId,
                DateTime.UtcNow
            );
        }

        /// <summary>
        /// Notify about routine session activity updates
        /// </summary>
        public async Task NotifySessionActivityChanged(int sessionId, int activityId, bool isCompleted, string deviceId)
        {
            await Clients.GroupExcept("SyncGroup", Context.ConnectionId).SendAsync(
                "SessionActivityChanged",
                sessionId,
                activityId,
                isCompleted,
                deviceId,
                DateTime.UtcNow
            );
        }

        /// <summary>
        /// Notify about timer events (start, pause, stop)
        /// </summary>
        public async Task NotifyTimerEvent(int activityId, string eventType, TimeSpan? duration, string deviceId)
        {
            await Clients.GroupExcept("SyncGroup", Context.ConnectionId).SendAsync(
                "TimerEvent",
                activityId,
                eventType, // "start", "pause", "stop", "complete"
                duration,
                deviceId,
                DateTime.UtcNow
            );
        }

        /// <summary>
        /// Send sync status updates
        /// </summary>
        public async Task NotifySyncStatus(string status, int pendingChanges, string deviceId)
        {
            await Clients.GroupExcept("SyncGroup", Context.ConnectionId).SendAsync(
                "SyncStatusChanged",
                status, // "syncing", "completed", "failed", "offline"
                pendingChanges,
                deviceId,
                DateTime.UtcNow
            );
        }

        /// <summary>
        /// Handle client connections
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            // Automatically join the global sync group
            await Groups.AddToGroupAsync(Context.ConnectionId, "SyncGroup");
            
            // Notify other clients about new connection
            await Clients.Others.SendAsync("DeviceConnected", Context.ConnectionId, DateTime.UtcNow);
            
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Handle client disconnections
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Notify other clients about disconnection
            await Clients.Others.SendAsync("DeviceDisconnected", Context.ConnectionId, DateTime.UtcNow);
            
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Request immediate sync from all connected devices
        /// </summary>
        public async Task RequestImmediateSync(string requestingDeviceId)
        {
            await Clients.GroupExcept("SyncGroup", Context.ConnectionId).SendAsync(
                "SyncRequested",
                requestingDeviceId,
                DateTime.UtcNow
            );
        }
    }
}