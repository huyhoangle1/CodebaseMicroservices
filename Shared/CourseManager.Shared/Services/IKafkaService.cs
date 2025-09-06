using CourseManager.Shared.DTOs.Notification;

namespace CourseManager.Shared.Services
{
    /// <summary>
    /// Service cho Kafka messaging
    /// </summary>
    public interface IKafkaService
    {
        // Producer methods
        Task<bool> PublishAsync<T>(string topic, T message, string? key = null);
        Task<bool> PublishAsync(string topic, string message, string? key = null);
        Task<bool> PublishBatchAsync<T>(string topic, List<T> messages, string? key = null);

        // Consumer methods
        Task StartConsumingAsync<T>(string topic, Func<T, Task> messageHandler, CancellationToken cancellationToken = default);
        Task StartConsumingAsync(string topic, Func<string, Task> messageHandler, CancellationToken cancellationToken = default);
        Task StopConsumingAsync();

        // Topic management
        Task<bool> CreateTopicAsync(string topic, int partitions = 1, short replicationFactor = 1);
        Task<bool> DeleteTopicAsync(string topic);
        Task<List<string>> GetTopicsAsync();

        // Event publishing
        Task PublishOrderEventAsync(OrderEventDto orderEvent);
        Task PublishPaymentEventAsync(PaymentEventDto paymentEvent);
        Task PublishCourseEventAsync(CourseEventDto courseEvent);
        Task PublishUserEventAsync(UserEventDto userEvent);
        Task PublishSystemEventAsync(SystemEventDto systemEvent);

        // Event consuming
        Task StartOrderEventConsumerAsync(Func<OrderEventDto, Task> handler, CancellationToken cancellationToken = default);
        Task StartPaymentEventConsumerAsync(Func<PaymentEventDto, Task> handler, CancellationToken cancellationToken = default);
        Task StartCourseEventConsumerAsync(Func<CourseEventDto, Task> handler, CancellationToken cancellationToken = default);
        Task StartUserEventConsumerAsync(Func<UserEventDto, Task> handler, CancellationToken cancellationToken = default);
        Task StartSystemEventConsumerAsync(Func<SystemEventDto, Task> handler, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// DTO cho sự kiện đơn hàng
    /// </summary>
    public class OrderEventDto
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public string EventType { get; set; } = string.Empty; // OrderCreated, OrderUpdated, OrderCancelled, OrderCompleted
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PreviousStatus { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// DTO cho sự kiện thanh toán
    /// </summary>
    public class PaymentEventDto
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public string EventType { get; set; } = string.Empty; // PaymentInitiated, PaymentCompleted, PaymentFailed, PaymentRefunded
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? TransactionId { get; set; }
        public string? PaymentReference { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// DTO cho sự kiện khóa học
    /// </summary>
    public class CourseEventDto
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public string EventType { get; set; } = string.Empty; // CourseCreated, CourseUpdated, CourseDeleted, CourseEnrolled
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string Instructor { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? UserId { get; set; } // For enrollment events
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// DTO cho sự kiện người dùng
    /// </summary>
    public class UserEventDto
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public string EventType { get; set; } = string.Empty; // UserRegistered, UserUpdated, UserDeleted, UserRoleChanged
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? PreviousRole { get; set; }
        public string? NewRole { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// DTO cho sự kiện hệ thống
    /// </summary>
    public class SystemEventDto
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public string EventType { get; set; } = string.Empty; // SystemStartup, SystemShutdown, MaintenanceStart, MaintenanceEnd, ErrorOccurred
        public string Level { get; set; } = "info"; // info, warning, error, critical
        public string Message { get; set; } = string.Empty;
        public string? Service { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object>? Metadata { get; set; }
    }
}
