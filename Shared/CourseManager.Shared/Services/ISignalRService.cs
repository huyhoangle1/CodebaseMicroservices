using CourseManager.Shared.DTOs.Notification;

namespace CourseManager.Shared.Services
{
    /// <summary>
    /// Service cho SignalR real-time communication
    /// </summary>
    public interface ISignalRService
    {
        // User-specific notifications
        Task SendToUserAsync(string userId, string method, object data);
        Task SendToUserAsync(string userId, NotificationDto notification);
        Task SendToUsersAsync(List<string> userIds, string method, object data);
        Task SendToUsersAsync(List<string> userIds, NotificationDto notification);

        // Group notifications
        Task SendToGroupAsync(string groupName, string method, object data);
        Task SendToGroupAsync(string groupName, NotificationDto notification);
        Task AddToGroupAsync(string connectionId, string groupName);
        Task RemoveFromGroupAsync(string connectionId, string groupName);

        // Role-based notifications
        Task SendToRoleAsync(string role, string method, object data);
        Task SendToRoleAsync(string role, NotificationDto notification);

        // Broadcast notifications
        Task SendToAllAsync(string method, object data);
        Task SendToAllAsync(NotificationDto notification);

        // Connection management
        Task OnConnectedAsync(string connectionId, string userId);
        Task OnDisconnectedAsync(string connectionId);
        Task<bool> IsUserOnlineAsync(string userId);
        Task<List<string>> GetOnlineUsersAsync();

        // Custom events
        Task NotifyOrderStatusChangedAsync(int orderId, string newStatus, string userId);
        Task NotifyPaymentStatusChangedAsync(int orderId, string paymentStatus, string userId);
        Task NotifyCourseCreatedAsync(int courseId, string courseTitle, List<string> userIds);
        Task NotifyUserRoleChangedAsync(string userId, string newRole);
        Task NotifySystemMaintenanceAsync(string message, DateTime scheduledTime);
    }
}
