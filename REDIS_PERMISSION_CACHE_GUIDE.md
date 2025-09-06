# Hướng dẫn sử dụng Redis Cache cho Quyền hạn

## 🚀 Tổng quan

Hệ thống cache quyền hạn sử dụng Redis để tối ưu hiệu suất, tránh việc query database quá nhiều khi kiểm tra quyền hạn.

## 🔧 Cài đặt

### 1. Cài đặt Redis
```bash
# Sử dụng Docker
docker run -d --name coursemanager-redis -p 6379:6379 redis:7-alpine

# Hoặc cài đặt Redis trên Windows
# Download từ: https://github.com/microsoftarchive/redis/releases
```

### 2. Cấu hình appsettings.json
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "PermissionCache": {
    "DefaultExpiry": "01:00:00",
    "UserPermissionExpiry": "02:00:00",
    "RolePermissionExpiry": "04:00:00",
    "MenuPermissionExpiry": "06:00:00",
    "MaxCacheSize": 10000,
    "EnableCompression": true,
    "EnableStatistics": true,
    "KeyPrefix": "coursemanager:permissions:"
  }
}
```

### 3. Cài đặt NuGet Packages
```xml
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.0" />
<PackageReference Include="StackExchange.Redis" Version="2.7.4" />
```

## 🏗️ Cấu hình Services

### 1. Program.cs Configuration
```csharp
// Thêm Redis cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "CourseManager";
});

// Cấu hình Permission Cache
builder.Services.Configure<PermissionCacheConfiguration>(
    builder.Configuration.GetSection("PermissionCache"));

// Đăng ký services
builder.Services.AddScoped<IPermissionCacheService, PermissionCacheService>();
builder.Services.AddScoped<IPermissionService, CachedPermissionService>();

// Thêm middleware
app.UsePermissionCache();
```

## 📊 Cấu trúc Cache

### 1. Cache Keys
```
coursemanager:permissions:user_permissions:{userId}
coursemanager:permissions:user_roles:{userId}
coursemanager:permissions:user_menus:{userId}
coursemanager:permissions:user_permission_matrix:{userId}
coursemanager:permissions:user_role_matrix:{userId}
coursemanager:permissions:role_permissions:{roleId}
```

### 2. Cache Data Format
```json
// User Permissions
["courses:create", "courses:read", "orders:update"]

// User Roles
["Admin", "Manager"]

// User Menus
[1, 2, 3, 5, 8]

// Permission Matrix
{
  "courses": ["create", "read", "update", "delete"],
  "orders": ["read", "update"],
  "users": ["read"]
}
```

## 🎯 Sử dụng

### 1. Kiểm tra quyền hạn với Cache
```csharp
public class OrdersController : ControllerBase
{
    private readonly IPermissionCacheService _cacheService;
    
    [HttpGet]
    [RequirePermission("orders", "read")]
    public async Task<IActionResult> GetOrders()
    {
        // Controller sẽ tự động kiểm tra quyền từ cache
        return Ok();
    }
}
```

### 2. Sử dụng Service trực tiếp
```csharp
public class OrderService
{
    private readonly IPermissionCacheService _cacheService;
    
    public async Task<bool> CanUserCreateOrderAsync(int userId)
    {
        // Kiểm tra từ cache trước, nếu không có sẽ query database
        return await _cacheService.UserHasPermissionAsync(userId, "orders", "create");
    }
    
    public async Task<List<string>> GetUserPermissionsAsync(int userId)
    {
        // Lấy danh sách quyền hạn từ cache
        return await _cacheService.GetUserPermissionsAsync(userId);
    }
}
```

### 3. Cache Invalidation
```csharp
public class PermissionService
{
    private readonly IPermissionCacheService _cacheService;
    
    public async Task AssignPermissionToUserAsync(int userId, int permissionId)
    {
        // Gán quyền hạn
        await _permissionRepository.AssignPermissionAsync(userId, permissionId);
        
        // Invalidate cache của user
        await _cacheService.InvalidateUserCacheAsync(userId);
    }
    
    public async Task UpdateRoleAsync(int roleId, UpdateRoleRequest request)
    {
        // Cập nhật role
        await _roleRepository.UpdateAsync(roleId, role);
        
        // Invalidate cache của role
        await _cacheService.InvalidateRoleCacheAsync(roleId);
    }
}
```

## 🔍 Monitoring và Debugging

### 1. Cache Statistics
```csharp
public class CacheController : ControllerBase
{
    private readonly IPermissionCacheService _cacheService;
    
    [HttpGet("cache/stats")]
    public async Task<IActionResult> GetCacheStatistics()
    {
        var stats = await _cacheService.GetCacheStatisticsAsync();
        return Ok(stats);
    }
    
    [HttpGet("cache/health")]
    public async Task<IActionResult> GetCacheHealth()
    {
        var isHealthy = await _cacheService.IsCacheHealthyAsync();
        return Ok(new { IsHealthy = isHealthy });
    }
}
```

### 2. Redis CLI Commands
```bash
# Kết nối Redis
redis-cli

# Xem tất cả keys
KEYS coursemanager:permissions:*

# Xem value của key
GET coursemanager:permissions:user_permissions:1

# Xem TTL của key
TTL coursemanager:permissions:user_permissions:1

# Xóa key
DEL coursemanager:permissions:user_permissions:1

# Xóa tất cả keys với pattern
EVAL "return redis.call('del', unpack(redis.call('keys', 'coursemanager:permissions:*')))" 0
```

## ⚡ Performance Benefits

### 1. Trước khi có Cache
```
Database Query: 50ms
Total Response Time: 50ms
Concurrent Users: 100
Total DB Load: 100 queries/second
```

### 2. Sau khi có Cache
```
Cache Hit: 1ms (95% cases)
Cache Miss: 51ms (5% cases)
Average Response Time: 3ms
Concurrent Users: 100
Total DB Load: 5 queries/second
```

### 3. Memory Usage
```
User Permissions: ~1KB per user
Role Permissions: ~2KB per role
Total for 1000 users: ~3MB
```

## 🚨 Best Practices

### 1. Cache Expiry Strategy
```csharp
// User permissions: 2 hours (thay đổi thường xuyên)
await _cacheService.SetUserPermissionsAsync(userId, permissions, TimeSpan.FromHours(2));

// Role permissions: 4 hours (ít thay đổi)
await _cacheService.SetRolePermissionsAsync(roleId, permissions, TimeSpan.FromHours(4));

// Menu permissions: 6 hours (rất ít thay đổi)
await _cacheService.SetUserMenusAsync(userId, menuIds, TimeSpan.FromHours(6));
```

### 2. Cache Invalidation
```csharp
// Khi user được gán role mới
await _cacheService.InvalidateUserCacheAsync(userId);

// Khi role được cập nhật
await _cacheService.InvalidateRoleCacheAsync(roleId);

// Khi permission được thay đổi
await _cacheService.InvalidatePermissionCacheAsync(permissionId);
```

### 3. Error Handling
```csharp
try
{
    var hasPermission = await _cacheService.UserHasPermissionAsync(userId, resource, action);
    return hasPermission;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Cache error, falling back to database");
    // Fallback to database query
    return await _permissionService.UserHasPermissionAsync(userId, resource, action);
}
```

## 🔧 Troubleshooting

### 1. Cache Miss Issues
```csharp
// Kiểm tra cache statistics
var stats = await _cacheService.GetCacheStatisticsAsync();
if (stats.HitRatio < 0.8)
{
    _logger.LogWarning("Low cache hit ratio: {HitRatio}", stats.HitRatio);
}
```

### 2. Memory Issues
```bash
# Kiểm tra Redis memory usage
redis-cli INFO memory

# Xóa cache nếu cần
redis-cli FLUSHDB
```

### 3. Connection Issues
```csharp
// Kiểm tra Redis connection
var isHealthy = await _cacheService.IsCacheHealthyAsync();
if (!isHealthy)
{
    _logger.LogError("Redis connection failed");
}
```

## 📈 Scaling

### 1. Redis Cluster
```json
{
  "ConnectionStrings": {
    "Redis": "redis-cluster:6379,redis-cluster:6380,redis-cluster:6381"
  }
}
```

### 2. Redis Sentinel
```json
{
  "ConnectionStrings": {
    "Redis": "sentinel1:26379,sentinel2:26379,sentinel3:26379,serviceName=mymaster"
  }
}
```

### 3. Cache Warming
```csharp
// Warm up cache khi khởi động ứng dụng
public class CacheWarmupService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _cacheService.WarmUpAllCachesAsync();
    }
}
```

## 🎉 Kết luận

Redis cache cho quyền hạn cung cấp:
- ✅ **Hiệu suất cao** - Giảm 95% database queries
- ✅ **Scalable** - Hỗ trợ hàng nghìn concurrent users
- ✅ **Reliable** - Fallback to database khi cache fail
- ✅ **Flexible** - Có thể cấu hình expiry time khác nhau
- ✅ **Monitorable** - Có statistics và health checks
- ✅ **Maintainable** - Dễ dàng invalidate cache khi cần

Hệ thống này đảm bảo:
- 🚀 **Fast response times** (< 5ms)
- 🔒 **Secure permission checking**
- 📊 **Real-time monitoring**
- 🛠️ **Easy maintenance**
