using CourseManager.Shared.DTOs.Notification;

namespace CourseManager.Shared.Services
{
    /// <summary>
    /// Service cache quyền hạn với Redis
    /// </summary>
    public interface IPermissionCacheService
    {
        // User permissions cache
        Task<List<string>> GetUserPermissionsAsync(int userId);
        Task SetUserPermissionsAsync(int userId, List<string> permissions, TimeSpan? expiry = null);
        Task<bool> UserHasPermissionAsync(int userId, string resource, string action);
        Task<bool> UserHasRoleAsync(int userId, string roleName);
        Task<List<string>> GetUserRolesAsync(int userId);
        Task SetUserRolesAsync(int userId, List<string> roles, TimeSpan? expiry = null);

        // Role permissions cache
        Task<List<string>> GetRolePermissionsAsync(int roleId);
        Task SetRolePermissionsAsync(int roleId, List<string> permissions, TimeSpan? expiry = null);
        Task<bool> RoleHasPermissionAsync(int roleId, string resource, string action);

        // Menu permissions cache
        Task<List<int>> GetUserMenusAsync(int userId);
        Task SetUserMenusAsync(int userId, List<int> menuIds, TimeSpan? expiry = null);
        Task<bool> UserCanAccessMenuAsync(int userId, int menuId);

        // Permission matrix cache
        Task<Dictionary<string, List<string>>> GetUserPermissionMatrixAsync(int userId);
        Task SetUserPermissionMatrixAsync(int userId, Dictionary<string, List<string>> matrix, TimeSpan? expiry = null);
        Task<Dictionary<string, List<string>>> GetUserRoleMatrixAsync(int userId);
        Task SetUserRoleMatrixAsync(int userId, Dictionary<string, List<string>> matrix, TimeSpan? expiry = null);

        // Cache invalidation
        Task InvalidateUserCacheAsync(int userId);
        Task InvalidateRoleCacheAsync(int roleId);
        Task InvalidatePermissionCacheAsync(int permissionId);
        Task InvalidateMenuCacheAsync(int menuId);
        Task InvalidateAllCacheAsync();

        // Cache statistics
        Task<CacheStatistics> GetCacheStatisticsAsync();
        Task<bool> IsCacheHealthyAsync();

        // Bulk operations
        Task<Dictionary<int, List<string>>> GetMultipleUserPermissionsAsync(List<int> userIds);
        Task SetMultipleUserPermissionsAsync(Dictionary<int, List<string>> userPermissions, TimeSpan? expiry = null);
        Task<Dictionary<int, List<string>>> GetMultipleRolePermissionsAsync(List<int> roleIds);
        Task SetMultipleRolePermissionsAsync(Dictionary<int, List<string>> rolePermissions, TimeSpan? expiry = null);

        // Cache warming
        Task WarmUpUserCacheAsync(int userId);
        Task WarmUpRoleCacheAsync(int roleId);
        Task WarmUpAllCachesAsync();
    }

    /// <summary>
    /// Thống kê cache
    /// </summary>
    public class CacheStatistics
    {
        public int TotalKeys { get; set; }
        public int UserPermissionKeys { get; set; }
        public int RolePermissionKeys { get; set; }
        public int MenuPermissionKeys { get; set; }
        public long MemoryUsage { get; set; }
        public int HitCount { get; set; }
        public int MissCount { get; set; }
        public double HitRatio => HitCount + MissCount > 0 ? (double)HitCount / (HitCount + MissCount) : 0;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Cấu hình cache
    /// </summary>
    public class PermissionCacheConfiguration
    {
        public TimeSpan DefaultExpiry { get; set; } = TimeSpan.FromHours(1);
        public TimeSpan UserPermissionExpiry { get; set; } = TimeSpan.FromHours(2);
        public TimeSpan RolePermissionExpiry { get; set; } = TimeSpan.FromHours(4);
        public TimeSpan MenuPermissionExpiry { get; set; } = TimeSpan.FromHours(6);
        public int MaxCacheSize { get; set; } = 10000;
        public bool EnableCompression { get; set; } = true;
        public bool EnableStatistics { get; set; } = true;
        public string KeyPrefix { get; set; } = "coursemanager:permissions:";
    }
}
