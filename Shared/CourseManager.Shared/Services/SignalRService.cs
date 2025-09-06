using Microsoft.AspNetCore.SignalR;
using CourseManager.Shared.DTOs.Notification;
using CourseManager.Shared.Hubs;
using System.Collections.Concurrent;

namespace CourseManager.Shared.Services
{
    /// <summary>
    /// Implementation của SignalR service
    /// </summary>
    public class SignalRService : ISignalRService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRService> _logger;
        private readonly ConcurrentDictionary<string, string> _userConnections = new();
        private readonly ConcurrentDictionary<string, List<string>> _userGroups = new();

        public SignalRService(IHubContext<NotificationHub> hubContext, ILogger<SignalRService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendToUserAsync(string userId, string method, object data)
        {
            try
            {
                if (_userConnections.TryGetValue(userId, out var connectionId))
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync(method, data);
                    _logger.LogDebug("Sent message to user {UserId} via connection {ConnectionId}", userId, connectionId);
                }
                else
                {
                    _logger.LogWarning("User {UserId} is not connected", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to user {UserId}", userId);
            }
        }

        public async Task SendToUserAsync(string userId, NotificationDto notification)
        {
            await SendToUserAsync(userId, "ReceiveNotification", notification);
        }

        public async Task SendToUsersAsync(List<string> userIds, string method, object data)
        {
            try
            {
                var tasks = userIds.Select(userId => SendToUserAsync(userId, method, data));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to multiple users");
            }
        }

        public async Task SendToUsersAsync(List<string> userIds, NotificationDto notification)
        {
            await SendToUsersAsync(userIds, "ReceiveNotification", notification);
        }

        public async Task SendToGroupAsync(string groupName, string method, object data)
        {
            try
            {
                await _hubContext.Clients.Group(groupName).SendAsync(method, data);
                _logger.LogDebug("Sent message to group {GroupName}", groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to group {GroupName}", groupName);
            }
        }

        public async Task SendToGroupAsync(string groupName, NotificationDto notification)
        {
            await SendToGroupAsync(groupName, "ReceiveNotification", notification);
        }

        public async Task AddToGroupAsync(string connectionId, string groupName)
        {
            try
            {
                await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
                _logger.LogDebug("Added connection {ConnectionId} to group {GroupName}", connectionId, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding connection {ConnectionId} to group {GroupName}", connectionId, groupName);
            }
        }

        public async Task RemoveFromGroupAsync(string connectionId, string groupName)
        {
            try
            {
                await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
                _logger.LogDebug("Removed connection {ConnectionId} from group {GroupName}", connectionId, groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing connection {ConnectionId} from group {GroupName}", connectionId, groupName);
            }
        }

        public async Task SendToRoleAsync(string role, string method, object data)
        {
            await SendToGroupAsync($"role_{role}", method, data);
        }

        public async Task SendToRoleAsync(string role, NotificationDto notification)
        {
            await SendToGroupAsync($"role_{role}", "ReceiveNotification", notification);
        }

        public async Task SendToAllAsync(string method, object data)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync(method, data);
                _logger.LogDebug("Sent message to all connected clients");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to all clients");
            }
        }

        public async Task SendToAllAsync(NotificationDto notification)
        {
            await SendToAllAsync("ReceiveNotification", notification);
        }

        public async Task OnConnectedAsync(string connectionId, string userId)
        {
            _userConnections[userId] = connectionId;
            _userGroups[userId] = new List<string>();
            _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, connectionId);
            await Task.CompletedTask;
        }

        public async Task OnDisconnectedAsync(string connectionId)
        {
            var userId = _userConnections.FirstOrDefault(x => x.Value == connectionId).Key;
            if (userId != null)
            {
                _userConnections.TryRemove(userId, out _);
                _userGroups.TryRemove(userId, out _);
                _logger.LogInformation("User {UserId} disconnected from connection {ConnectionId}", userId, connectionId);
            }
            await Task.CompletedTask;
        }

        public async Task<bool> IsUserOnlineAsync(string userId)
        {
            return await Task.FromResult(_userConnections.ContainsKey(userId));
        }

        public async Task<List<string>> GetOnlineUsersAsync()
        {
            return await Task.FromResult(_userConnections.Keys.ToList());
        }

        public async Task NotifyOrderStatusChangedAsync(int orderId, string newStatus, string userId)
        {
            var notification = new OrderNotificationDto
            {
                OrderId = orderId,
                OrderNumber = $"ORD-{orderId:D6}",
                Status = newStatus,
                Title = "Order Status Updated",
                Message = $"Your order #{orderId} status has been updated to {newStatus}",
                Type = "info",
                UserId = userId,
                Url = $"/orders/{orderId}"
            };

            await SendToUserAsync(userId, notification);
            await SendToGroupAsync($"order_{orderId}", notification);
        }

        public async Task NotifyPaymentStatusChangedAsync(int orderId, string paymentStatus, string userId)
        {
            var notification = new PaymentNotificationDto
            {
                OrderId = orderId,
                OrderNumber = $"ORD-{orderId:D6}",
                PaymentStatus = paymentStatus,
                Title = "Payment Status Updated",
                Message = $"Payment for order #{orderId} status has been updated to {paymentStatus}",
                Type = paymentStatus == "Completed" ? "success" : "warning",
                UserId = userId,
                Url = $"/orders/{orderId}"
            };

            await SendToUserAsync(userId, notification);
            await SendToGroupAsync($"order_{orderId}", notification);
        }

        public async Task NotifyCourseCreatedAsync(int courseId, string courseTitle, List<string> userIds)
        {
            var notification = new CourseNotificationDto
            {
                CourseId = courseId,
                CourseTitle = courseTitle,
                Action = "created",
                Title = "New Course Available",
                Message = $"A new course '{courseTitle}' has been added",
                Type = "info",
                Url = $"/courses/{courseId}"
            };

            await SendToUsersAsync(userIds, notification);
            await SendToGroupAsync($"course_{courseId}", notification);
        }

        public async Task NotifyUserRoleChangedAsync(string userId, string newRole)
        {
            var notification = new UserNotificationDto
            {
                UserId = userId,
                Action = "role_changed",
                Title = "Role Updated",
                Message = $"Your role has been changed to {newRole}",
                Type = "info",
                UserId = userId
            };

            await SendToUserAsync(userId, notification);
        }

        public async Task NotifySystemMaintenanceAsync(string message, DateTime scheduledTime)
        {
            var notification = new SystemNotificationDto
            {
                Title = "System Maintenance",
                Message = message,
                Type = "warning",
                Priority = "high",
                ScheduledTime = scheduledTime,
                IsMaintenance = true
            };

            await SendToAllAsync(notification);
        }
    }
}
