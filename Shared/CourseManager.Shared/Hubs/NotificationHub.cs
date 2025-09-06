using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using CourseManager.Shared.DTOs.Notification;
using System.Security.Claims;

namespace CourseManager.Shared.Hubs
{
    /// <summary>
    /// SignalR Hub cho real-time notifications
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;
        private readonly ISignalRService _signalRService;

        public NotificationHub(ILogger<NotificationHub> logger, ISignalRService signalRService)
        {
            _logger = logger;
            _signalRService = signalRService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                await _signalRService.OnConnectedAsync(Context.ConnectionId, userId);
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                
                // Join role-based groups
                var role = GetUserRole();
                if (!string.IsNullOrEmpty(role))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{role}");
                }

                _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                await _signalRService.OnDisconnectedAsync(Context.ConnectionId);
                _logger.LogInformation("User {UserId} disconnected from connection {ConnectionId}", userId, Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join a specific group
        /// </summary>
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await _signalRService.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("User {UserId} joined group {GroupName}", GetUserId(), groupName);
        }

        /// <summary>
        /// Leave a specific group
        /// </summary>
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await _signalRService.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation("User {UserId} left group {GroupName}", GetUserId(), groupName);
        }

        /// <summary>
        /// Join order tracking group
        /// </summary>
        public async Task JoinOrderTracking(int orderId)
        {
            var groupName = $"order_{orderId}";
            await JoinGroup(groupName);
        }

        /// <summary>
        /// Leave order tracking group
        /// </summary>
        public async Task LeaveOrderTracking(int orderId)
        {
            var groupName = $"order_{orderId}";
            await LeaveGroup(groupName);
        }

        /// <summary>
        /// Join course updates group
        /// </summary>
        public async Task JoinCourseUpdates(int courseId)
        {
            var groupName = $"course_{courseId}";
            await JoinGroup(groupName);
        }

        /// <summary>
        /// Leave course updates group
        /// </summary>
        public async Task LeaveCourseUpdates(int courseId)
        {
            var groupName = $"course_{courseId}";
            await LeaveGroup(groupName);
        }

        /// <summary>
        /// Mark notification as read
        /// </summary>
        public async Task MarkNotificationAsRead(string notificationId)
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                // Implement notification read logic
                _logger.LogInformation("User {UserId} marked notification {NotificationId} as read", userId, notificationId);
            }
        }

        /// <summary>
        /// Get online users count
        /// </summary>
        public async Task<int> GetOnlineUsersCount()
        {
            // This would typically be implemented with a more sophisticated approach
            // For now, we'll return a simple count
            return await Task.FromResult(1);
        }

        private string? GetUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private string? GetUserRole()
        {
            return Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}
