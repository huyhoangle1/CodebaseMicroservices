namespace CourseManager.Shared.Configuration
{
    /// <summary>
    /// Cấu hình Kafka
    /// </summary>
    public class KafkaConfiguration
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string GroupId { get; set; } = "coursemanager-group";
        public string ClientId { get; set; } = "coursemanager-client";
        public bool EnableAutoCommit { get; set; } = true;
        public int AutoCommitIntervalMs { get; set; } = 1000;
        public int SessionTimeoutMs { get; set; } = 30000;
        public int RequestTimeoutMs { get; set; } = 30000;
        public int RetryBackoffMs { get; set; } = 100;
        public int MaxRetryAttempts { get; set; } = 3;
        public string SecurityProtocol { get; set; } = "PLAINTEXT";
        public string? SaslMechanism { get; set; }
        public string? SaslUsername { get; set; }
        public string? SaslPassword { get; set; }
        public string? SslCaLocation { get; set; }
        public string? SslCertificateLocation { get; set; }
        public string? SslKeyLocation { get; set; }
        public string? SslKeyPassword { get; set; }
    }

    /// <summary>
    /// Cấu hình topics Kafka
    /// </summary>
    public class KafkaTopics
    {
        public string OrderEvents { get; set; } = "order-events";
        public string PaymentEvents { get; set; } = "payment-events";
        public string CourseEvents { get; set; } = "course-events";
        public string UserEvents { get; set; } = "user-events";
        public string SystemEvents { get; set; } = "system-events";
        public string Notifications { get; set; } = "notifications";
        public string AuditLogs { get; set; } = "audit-logs";
        public string EmailQueue { get; set; } = "email-queue";
        public string SmsQueue { get; set; } = "sms-queue";
        public string PushNotifications { get; set; } = "push-notifications";
    }

    /// <summary>
    /// Cấu hình SignalR
    /// </summary>
    public class SignalRConfiguration
    {
        public bool EnableDetailedErrors { get; set; } = false;
        public int MaximumReceiveMessageSize { get; set; } = 32 * 1024; // 32KB
        public int StreamBufferCapacity { get; set; } = 10;
        public int ClientTimeoutInterval { get; set; } = 30; // seconds
        public int KeepAliveInterval { get; set; } = 15; // seconds
        public int HandshakeTimeout { get; set; } = 15; // seconds
        public bool EnableCompression { get; set; } = true;
        public string? RedisConnectionString { get; set; }
        public bool UseRedisBackplane { get; set; } = false;
    }

    /// <summary>
    /// Cấu hình real-time features
    /// </summary>
    public class RealtimeConfiguration
    {
        public bool EnableSignalR { get; set; } = true;
        public bool EnableKafka { get; set; } = true;
        public bool EnableWebSockets { get; set; } = true;
        public bool EnableServerSentEvents { get; set; } = false;
        public int MaxConnectionsPerUser { get; set; } = 5;
        public int MaxGroupsPerUser { get; set; } = 10;
        public int NotificationRetentionDays { get; set; } = 30;
        public bool EnableNotificationPersistence { get; set; } = true;
        public bool EnableEventSourcing { get; set; } = false;
    }
}
