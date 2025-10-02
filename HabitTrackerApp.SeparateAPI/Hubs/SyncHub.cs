using Microsoft.AspNetCore.SignalR;
using HabitTrackerApp.Core.Services.Sync;

namespace HabitTrackerApp.SeparateAPI.Hubs
{
    /// <summary>
    /// SignalR hub for real-time synchronization between devices
    /// </summary>
    public class SyncHub : Hub
    {
        /// <summary>
        /// Join the sync group for receiving real-time updates
        /// </summary>
        /// <param name="deviceId">Unique device identifier</param>
        public async Task JoinSyncGroup(string deviceId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "SyncGroup");
            
            // Store device ID in connection context
            Context.Items["DeviceId"] = deviceId;
            
            // Notify other clients about new device joining
            await Clients.Others.SendAsync("DeviceConnected", deviceId);
        }

        /// <summary>
        /// Leave the sync group
        /// </summary>
        public async Task LeaveSyncGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SyncGroup");
            
            if (Context.Items.TryGetValue("DeviceId", out var deviceId))
            {
                await Clients.Others.SendAsync("DeviceDisconnected", deviceId);
            }
        }

        /// <summary>
        /// Notify all other devices about data changes
        /// </summary>
        /// <param name="tableName">Name of the changed table</param>
        /// <param name="recordId">ID of the changed record</param>
        /// <param name="operation">Type of operation (INSERT, UPDATE, DELETE)</param>
        /// <param name="data">The changed data (for INSERT/UPDATE)</param>
        public async Task NotifyDataChanged(string tableName, int recordId, string operation, object? data = null)
        {
            var deviceId = Context.Items["DeviceId"]?.ToString();
            
            // Send change notification to all other devices in sync group (excluding sender)
            await Clients.OthersInGroup("SyncGroup").SendAsync("DataChanged", new
            {
                TableName = tableName,
                RecordId = recordId,
                Operation = operation,
                Data = data,
                DeviceId = deviceId,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Broadcast sync completion status
        /// </summary>
        /// <param name="syncResult">Result of the sync operation</param>
        public async Task NotifySyncCompleted(SyncResult syncResult)
        {
            var deviceId = Context.Items["DeviceId"]?.ToString();
            
            await Clients.OthersInGroup("SyncGroup").SendAsync("SyncCompleted", new
            {
                DeviceId = deviceId,
                Success = syncResult.Success,
                Message = syncResult.Message,
                SyncedRecords = syncResult.SyncedRecords,
                Timestamp = syncResult.SyncTimestamp
            });
        }

        /// <summary>
        /// Request immediate sync from all connected devices
        /// </summary>
        public async Task RequestSync()
        {
            var deviceId = Context.Items["DeviceId"]?.ToString();
            
            await Clients.OthersInGroup("SyncGroup").SendAsync("SyncRequested", new
            {
                RequestedBy = deviceId,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Handle client disconnection
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.Items.TryGetValue("DeviceId", out var deviceId))
            {
                await Clients.Others.SendAsync("DeviceDisconnected", deviceId);
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Get list of connected devices
        /// </summary>
        public async Task GetConnectedDevices()
        {
            // This would require storing connected devices in a service
            // For now, just acknowledge the request
            await Clients.Caller.SendAsync("ConnectedDevicesResponse", new[] { "Device list not implemented yet" });
        }
    }
}