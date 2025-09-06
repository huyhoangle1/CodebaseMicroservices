namespace CourseManager.Shared.DTOs.Notification
{
    /// <summary>
    /// DTO cho thông báo real-time
    /// </summary>
    public class NotificationDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info"; // info, success, warning, error
        public string? Icon { get; set; }
        public string? Url { get; set; }
        public Dictionary<string, object>? Data { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public bool IsRead { get; set; } = false;
        public string? UserId { get; set; }
        public string? GroupName { get; set; }
    }

    /// <summary>
    /// DTO cho thông báo đơn hàng
    /// </summary>
    public class OrderNotificationDto : NotificationDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// DTO cho thông báo thanh toán
    /// </summary>
    public class PaymentNotificationDto : NotificationDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// DTO cho thông báo khóa học
    /// </summary>
    public class CourseNotificationDto : NotificationDto
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // created, updated, deleted, enrolled
    }

    /// <summary>
    /// DTO cho thông báo hệ thống
    /// </summary>
    public class SystemNotificationDto : NotificationDto
    {
        public string Priority { get; set; } = "normal"; // low, normal, high, critical
        public DateTime? ScheduledTime { get; set; }
        public bool IsMaintenance { get; set; } = false;
    }

    /// <summary>
    /// DTO cho thông báo người dùng
    /// </summary>
    public class UserNotificationDto : NotificationDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // registered, updated, deleted, role_changed
    }

    /// <summary>
    /// DTO cho danh sách thông báo
    /// </summary>
    public class NotificationListDto
    {
        public List<NotificationDto> Notifications { get; set; } = new List<NotificationDto>();
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO cho cài đặt thông báo
    /// </summary>
    public class NotificationSettingsDto
    {
        public string UserId { get; set; } = string.Empty;
        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = true;
        public bool OrderNotifications { get; set; } = true;
        public bool PaymentNotifications { get; set; } = true;
        public bool CourseNotifications { get; set; } = true;
        public bool SystemNotifications { get; set; } = true;
        public List<string> NotificationTypes { get; set; } = new List<string>();
    }
}
