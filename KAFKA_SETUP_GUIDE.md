# Hướng dẫn cài đặt và sử dụng Kafka + SignalR

## 🚀 Cài đặt Kafka với Docker

### 1. Chạy Kafka Stack
```bash
# Chạy Kafka, Zookeeper, Redis và Kafka UI
docker-compose -f docker-compose.kafka.yml up -d

# Kiểm tra trạng thái
docker-compose -f docker-compose.kafka.yml ps
```

### 2. Truy cập Kafka UI
- URL: http://localhost:8080
- Xem topics, messages, consumers, producers

### 3. Cài đặt .NET Packages
```xml
<!-- Thêm vào .csproj files -->
<PackageReference Include="Confluent.Kafka" Version="2.3.0" />
<PackageReference Include="Microsoft.AspNetCore.SignalR" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="8.0.0" />
<PackageReference Include="StackExchange.Redis" Version="2.7.4" />
```

## 🔧 Cấu hình

### 1. Cập nhật appsettings.json
```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "coursemanager-group",
    "ClientId": "coursemanager-client"
  },
  "SignalR": {
    "EnableDetailedErrors": true,
    "UseRedisBackplane": true
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

### 2. Cấu hình Program.cs
```csharp
// Thêm SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 32 * 1024;
})
.AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
});

// Thêm Kafka
builder.Services.Configure<KafkaConfiguration>(
    builder.Configuration.GetSection("Kafka"));
builder.Services.Configure<KafkaTopics>(
    builder.Configuration.GetSection("KafkaTopics"));

builder.Services.AddSingleton<IKafkaService, KafkaService>();
builder.Services.AddSingleton<ISignalRService, SignalRService>();

// Thêm Redis (optional)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
```

## 📡 Sử dụng SignalR

### 1. Client-side JavaScript
```javascript
// Kết nối SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

// Bắt đầu kết nối
connection.start().then(function () {
    console.log("SignalR Connected");
}).catch(function (err) {
    console.error("SignalR Connection Error: ", err);
});

// Lắng nghe thông báo
connection.on("ReceiveNotification", function (notification) {
    console.log("New notification:", notification);
    showNotification(notification);
});

// Tham gia group
connection.invoke("JoinOrderTracking", orderId);
connection.invoke("JoinCourseUpdates", courseId);
```

### 2. Server-side Usage
```csharp
// Trong Controller hoặc Service
public class OrdersController : ControllerBase
{
    private readonly ISignalRService _signalRService;
    
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusRequest request)
    {
        // Update order logic...
        
        // Gửi thông báo real-time
        await _signalRService.NotifyOrderStatusChangedAsync(
            id, request.Status, userId);
            
        return Ok();
    }
}
```

## 📨 Sử dụng Kafka

### 1. Publishing Events
```csharp
public class OrderService
{
    private readonly IKafkaService _kafkaService;
    
    public async Task CreateOrderAsync(Order order)
    {
        // Create order logic...
        
        // Publish event
        var orderEvent = new OrderEventDto
        {
            EventType = "OrderCreated",
            OrderId = order.Id,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            Status = order.Status
        };
        
        await _kafkaService.PublishOrderEventAsync(orderEvent);
    }
}
```

### 2. Consuming Events
```csharp
public class NotificationService : BackgroundService
{
    private readonly IKafkaService _kafkaService;
    private readonly ISignalRService _signalRService;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Consume order events
        await _kafkaService.StartOrderEventConsumerAsync(async orderEvent =>
        {
            // Xử lý event
            await ProcessOrderEventAsync(orderEvent);
        }, stoppingToken);
    }
    
    private async Task ProcessOrderEventAsync(OrderEventDto orderEvent)
    {
        // Gửi thông báo real-time
        await _signalRService.NotifyOrderStatusChangedAsync(
            orderEvent.OrderId, 
            orderEvent.Status, 
            orderEvent.UserId.ToString());
    }
}
```

## 🎯 Use Cases

### 1. Real-time Notifications
- Thông báo trạng thái đơn hàng
- Thông báo thanh toán
- Thông báo khóa học mới
- Thông báo hệ thống

### 2. Event Sourcing
- Lưu trữ tất cả events
- Replay events để tái tạo state
- Audit trail

### 3. Microservices Communication
- Async communication giữa services
- Event-driven architecture
- Decoupling services

### 4. Analytics & Monitoring
- Track user behavior
- System metrics
- Business intelligence

## 🔍 Monitoring

### 1. Kafka UI
- Topics management
- Message inspection
- Consumer lag monitoring
- Producer metrics

### 2. Application Logs
```csharp
// Log Kafka events
_logger.LogInformation("Published order event {OrderId} to topic {Topic}", 
    orderId, "order-events");

// Log SignalR connections
_logger.LogInformation("User {UserId} connected with connection {ConnectionId}", 
    userId, connectionId);
```

### 3. Health Checks
```csharp
// Thêm health checks
builder.Services.AddHealthChecks()
    .AddCheck<KafkaHealthCheck>("kafka")
    .AddCheck<RedisHealthCheck>("redis");
```

## 🚨 Troubleshooting

### 1. Kafka Connection Issues
```bash
# Kiểm tra Kafka status
docker logs coursemanager-kafka

# Test connection
kafka-console-producer --bootstrap-server localhost:9092 --topic test-topic
```

### 2. SignalR Connection Issues
- Kiểm tra CORS configuration
- Verify authentication
- Check WebSocket support

### 3. Performance Issues
- Monitor consumer lag
- Adjust batch sizes
- Scale consumers horizontally

## 📊 Best Practices

### 1. Kafka
- Use appropriate partition keys
- Handle consumer failures gracefully
- Monitor consumer lag
- Use idempotent producers

### 2. SignalR
- Implement reconnection logic
- Use groups for targeted messaging
- Handle connection state
- Implement rate limiting

### 3. Error Handling
```csharp
try
{
    await _kafkaService.PublishAsync(topic, message);
}
catch (KafkaException ex)
{
    _logger.LogError(ex, "Failed to publish message to Kafka");
    // Implement retry logic or dead letter queue
}
```

## 🔐 Security

### 1. Kafka Security
- Enable SASL authentication
- Use SSL/TLS encryption
- Implement ACLs

### 2. SignalR Security
- Use JWT authentication
- Implement authorization
- Validate message content

## 📈 Scaling

### 1. Horizontal Scaling
- Multiple Kafka brokers
- Multiple consumer instances
- Load balancing

### 2. Vertical Scaling
- Increase memory
- More CPU cores
- Faster storage

## 🎉 Kết luận

Kafka + SignalR cung cấp:
- **Real-time communication** với SignalR
- **Reliable messaging** với Kafka
- **Scalable architecture** cho microservices
- **Event-driven design** cho flexibility
- **Monitoring & observability** cho production

Hệ thống này đảm bảo:
- ✅ High availability
- ✅ Fault tolerance
- ✅ Scalability
- ✅ Real-time updates
- ✅ Event sourcing
- ✅ Microservices communication
