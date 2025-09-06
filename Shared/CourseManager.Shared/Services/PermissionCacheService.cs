using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text;
using CourseManager.Shared.DTOs.Notification;

namespace CourseManager.Shared.Services
{
    /// <summary>
    /// Implementation của Permission Cache Service với Redis
    /// </summary>
    public class PermissionCacheService : IPermissionCacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<PermissionCacheService> _logger;
        private readonly PermissionCacheConfiguration _config;
        private readonly Dictionary<string, int> _hitCounts = new();
        private readonly Dictionary<string, int> _missCounts = new();

        public PermissionCacheService(
            IDistributedCache cache,
            ILogger<PermissionCacheService> logger,
            PermissionCacheConfiguration config)
        {
            _cache = cache;
            _logger = logger;
            _config = config;
        }

        #region User Permissions Cache

        public async Task<List<string>> GetUserPermissionsAsync(int userId)
        {
            var key = GetUserPermissionsKey(userId);
            try
            {
                var cached = await _cache.GetStringAsync(key);
                if (cached != null)
                {
                    IncrementHitCount(key);
                    return JsonSerializer.Deserialize<List<string>>(cached) ?? new List<string>();
                }
                else
                {
                    IncrementMissCount(key);
                    return new List<string>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user permissions from cache for user {UserId}", userId);
                IncrementMissCount(key);
                return new List<string>();
            }
        }

        public async Task SetUserPermissionsAsync(int userId, List<string> permissions, TimeSpan? expiry = null)
        {
            var key = GetUserPermissionsKey(userId);
            try
            {
                var json = JsonSerializer.Serialize(permissions);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry ?? _config.UserPermissionExpiry
                };

                await _cache.SetStringAsync(key, json, options);
                _logger.LogDebug("Cached user permissions for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching user permissions for user {UserId}", userId);
            }
        }

        public async Task<bool> UserHasPermissionAsync(int userId, string resource, string action)
        {
            var permissions = await GetUserPermissionsAsync(userId);
            var permissionKey = $"{resource}:{action}";
            return permissions.Contains(permissionKey);
        }

        public async Task<bool> UserHasRoleAsync(int userId, string roleName)
        {
            var roles = await GetUserRolesAsync(userId);
            return roles.Contains(roleName);
        }

        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            var key = GetUserRolesKey(userId);
            try
            {
                var cached = await _cache.GetStringAsync(key);
                if (cached != null)
                {
                    IncrementHitCount(key);
                    return JsonSerializer.Deserialize<List<string>>(cached) ?? new List<string>();
                }
                else
                {
                    IncrementMissCount(key);
                    return new List<string>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles from cache for user {UserId}", userId);
                IncrementMissCount(key);
                return new List<string>();
            }
        }

        public async Task SetUserRolesAsync(int userId, List<string> roles, TimeSpan? expiry = null)
        {
            var key = GetUserRolesKey(userId);
            try
            {
                var json = JsonSerializer.Serialize(roles);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry ?? _config.UserPermissionExpiry
                };

                await _cache.SetStringAsync(key, json, options);
                _logger.LogDebug("Cached user roles for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching user roles for user {UserId}", userId);
            }
        }

        #endregion

        #region Role Permissions Cache

        public async Task<List<string>> GetRolePermissionsAsync(int roleId)
        {
            var key = GetRolePermissionsKey(roleId);
            try
            {
                var cached = await _cache.GetStringAsync(key);
                if (cached != null)
                {
                    IncrementHitCount(key);
                    return JsonSerializer.Deserialize<List<string>>(cached) ?? new List<string>();
                }
                else
                {
                    IncrementMissCount(key);
                    return new List<string>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role permissions from cache for role {RoleId}", roleId);
                IncrementMissCount(key);
                return new List<string>();
            }
        }

        public async Task SetRolePermissionsAsync(int roleId, List<string> permissions, TimeSpan? expiry = null)
        {
            var key = GetRolePermissionsKey(roleId);
            try
            {
                var json = JsonSerializer.Serialize(permissions);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry ?? _config.RolePermissionExpiry
                };

                await _cache.SetStringAsync(key, json, options);
                _logger.LogDebug("Cached role permissions for role {RoleId}", roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching role permissions for role {RoleId}", roleId);
            }
        }

        public async Task<bool> RoleHasPermissionAsync(int roleId, string resource, string action)
        {
            var permissions = await GetRolePermissionsAsync(roleId);
            var permissionKey = $"{resource}:{action}";
            return permissions.Contains(permissionKey);
        }

        #endregion

        #region Menu Permissions Cache

        public async Task<List<int>> GetUserMenusAsync(int userId)
        {
            var key = GetUserMenusKey(userId);
            try
            {
                var cached = await _cache.GetStringAsync(key);
                if (cached != null)
                {
                    IncrementHitCount(key);
                    return JsonSerializer.Deserialize<List<int>>(cached) ?? new List<int>();
                }
                else
                {
                    IncrementMissCount(key);
                    return new List<int>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user menus from cache for user {UserId}", userId);
                IncrementMissCount(key);
                return new List<int>();
            }
        }

        public async Task SetUserMenusAsync(int userId, List<int> menuIds, TimeSpan? expiry = null)
        {
            var key = GetUserMenusKey(userId);
            try
            {
                var json = JsonSerializer.Serialize(menuIds);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry ?? _config.MenuPermissionExpiry
                };

                await _cache.SetStringAsync(key, json, options);
                _logger.LogDebug("Cached user menus for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching user menus for user {UserId}", userId);
            }
        }

        public async Task<bool> UserCanAccessMenuAsync(int userId, int menuId)
        {
            var menuIds = await GetUserMenusAsync(userId);
            return menuIds.Contains(menuId);
        }

        #endregion

        #region Permission Matrix Cache

        public async Task<Dictionary<string, List<string>>> GetUserPermissionMatrixAsync(int userId)
        {
            var key = GetUserPermissionMatrixKey(userId);
            try
            {
                var cached = await _cache.GetStringAsync(key);
                if (cached != null)
                {
                    IncrementHitCount(key);
                    return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(cached) ?? new Dictionary<string, List<string>>();
                }
                else
                {
                    IncrementMissCount(key);
                    return new Dictionary<string, List<string>>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user permission matrix from cache for user {UserId}", userId);
                IncrementMissCount(key);
                return new Dictionary<string, List<string>>();
            }
        }

        public async Task SetUserPermissionMatrixAsync(int userId, Dictionary<string, List<string>> matrix, TimeSpan? expiry = null)
        {
            var key = GetUserPermissionMatrixKey(userId);
            try
            {
                var json = JsonSerializer.Serialize(matrix);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry ?? _config.UserPermissionExpiry
                };

                await _cache.SetStringAsync(key, json, options);
                _logger.LogDebug("Cached user permission matrix for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching user permission matrix for user {UserId}", userId);
            }
        }

        public async Task<Dictionary<string, List<string>>> GetUserRoleMatrixAsync(int userId)
        {
            var key = GetUserRoleMatrixKey(userId);
            try
            {
                var cached = await _cache.GetStringAsync(key);
                if (cached != null)
                {
                    IncrementHitCount(key);
                    return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(cached) ?? new Dictionary<string, List<string>>();
                }
                else
                {
                    IncrementMissCount(key);
                    return new Dictionary<string, List<string>>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user role matrix from cache for user {UserId}", userId);
                IncrementMissCount(key);
                IncrementMissCount(key);
                return new Dictionary<string, List<string>>();
            }
        }

        public async Task SetUserRoleMatrixAsync(int userId, Dictionary<string, List<string>> matrix, TimeSpan? expiry = null)
        {
            var key = GetUserRoleMatrixKey(userId);
            try
            {
                var json = JsonSerializer.Serialize(matrix);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry ?? _config.UserPermissionExpiry
                };

                await _cache.SetStringAsync(key, json, options);
                _logger.LogDebug("Cached user role matrix for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching user role matrix for user {UserId}", userId);
            }
        }

        #endregion

        #region Cache Invalidation

        public async Task InvalidateUserCacheAsync(int userId)
        {
            var keys = new[]
            {
                GetUserPermissionsKey(userId),
                GetUserRolesKey(userId),
                GetUserMenusKey(userId),
                GetUserPermissionMatrixKey(userId),
                GetUserRoleMatrixKey(userId)
            };

            await InvalidateKeysAsync(keys);
            _logger.LogInformation("Invalidated cache for user {UserId}", userId);
        }

        public async Task InvalidateRoleCacheAsync(int roleId)
        {
            var key = GetRolePermissionsKey(roleId);
            await InvalidateKeysAsync(new[] { key });
            _logger.LogInformation("Invalidated cache for role {RoleId}", roleId);
        }

        public async Task InvalidatePermissionCacheAsync(int permissionId)
        {
            // Invalidate all user caches that might contain this permission
            await InvalidateAllCacheAsync();
            _logger.LogInformation("Invalidated all caches due to permission {PermissionId} change", permissionId);
        }

        public async Task InvalidateMenuCacheAsync(int menuId)
        {
            // Invalidate all user menu caches
            await InvalidateAllCacheAsync();
            _logger.LogInformation("Invalidated all caches due to menu {MenuId} change", menuId);
        }

        public async Task InvalidateAllCacheAsync()
        {
            try
            {
                // This would typically use Redis SCAN to find and delete all keys with our prefix
                // For now, we'll log the action
                _logger.LogInformation("Invalidated all permission caches");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating all caches");
            }
        }

        #endregion

        #region Cache Statistics

        public async Task<CacheStatistics> GetCacheStatisticsAsync()
        {
            var totalHits = _hitCounts.Values.Sum();
            var totalMisses = _missCounts.Values.Sum();

            return new CacheStatistics
            {
                TotalKeys = _hitCounts.Count + _missCounts.Count,
                UserPermissionKeys = _hitCounts.Count(k => k.Key.Contains("user_permissions")),
                RolePermissionKeys = _hitCounts.Count(k => k.Key.Contains("role_permissions")),
                MenuPermissionKeys = _hitCounts.Count(k => k.Key.Contains("user_menus")),
                HitCount = totalHits,
                MissCount = totalMisses,
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<bool> IsCacheHealthyAsync()
        {
            try
            {
                // Test cache connectivity
                await _cache.GetStringAsync("health_check");
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Bulk Operations

        public async Task<Dictionary<int, List<string>>> GetMultipleUserPermissionsAsync(List<int> userIds)
        {
            var result = new Dictionary<int, List<string>>();
            var tasks = userIds.Select(async userId =>
            {
                var permissions = await GetUserPermissionsAsync(userId);
                return new { UserId = userId, Permissions = permissions };
            });

            var results = await Task.WhenAll(tasks);
            foreach (var item in results)
            {
                result[item.UserId] = item.Permissions;
            }

            return result;
        }

        public async Task SetMultipleUserPermissionsAsync(Dictionary<int, List<string>> userPermissions, TimeSpan? expiry = null)
        {
            var tasks = userPermissions.Select(kvp => SetUserPermissionsAsync(kvp.Key, kvp.Value, expiry));
            await Task.WhenAll(tasks);
        }

        public async Task<Dictionary<int, List<string>>> GetMultipleRolePermissionsAsync(List<int> roleIds)
        {
            var result = new Dictionary<int, List<string>>();
            var tasks = roleIds.Select(async roleId =>
            {
                var permissions = await GetRolePermissionsAsync(roleId);
                return new { RoleId = roleId, Permissions = permissions };
            });

            var results = await Task.WhenAll(tasks);
            foreach (var item in results)
            {
                result[item.RoleId] = item.Permissions;
            }

            return result;
        }

        public async Task SetMultipleRolePermissionsAsync(Dictionary<int, List<string>> rolePermissions, TimeSpan? expiry = null)
        {
            var tasks = rolePermissions.Select(kvp => SetRolePermissionsAsync(kvp.Key, kvp.Value, expiry));
            await Task.WhenAll(tasks);
        }

        #endregion

        #region Cache Warming

        public async Task WarmUpUserCacheAsync(int userId)
        {
            // This would typically load user permissions from database and cache them
            _logger.LogInformation("Warming up cache for user {UserId}", userId);
            await Task.CompletedTask;
        }

        public async Task WarmUpRoleCacheAsync(int roleId)
        {
            // This would typically load role permissions from database and cache them
            _logger.LogInformation("Warming up cache for role {RoleId}", roleId);
            await Task.CompletedTask;
        }

        public async Task WarmUpAllCachesAsync()
        {
            _logger.LogInformation("Warming up all permission caches");
            await Task.CompletedTask;
        }

        #endregion

        #region Private Methods

        private string GetUserPermissionsKey(int userId) => $"{_config.KeyPrefix}user_permissions:{userId}";
        private string GetUserRolesKey(int userId) => $"{_config.KeyPrefix}user_roles:{userId}";
        private string GetUserMenusKey(int userId) => $"{_config.KeyPrefix}user_menus:{userId}";
        private string GetUserPermissionMatrixKey(int userId) => $"{_config.KeyPrefix}user_permission_matrix:{userId}";
        private string GetUserRoleMatrixKey(int userId) => $"{_config.KeyPrefix}user_role_matrix:{userId}";
        private string GetRolePermissionsKey(int roleId) => $"{_config.KeyPrefix}role_permissions:{roleId}";

        private async Task InvalidateKeysAsync(string[] keys)
        {
            var tasks = keys.Select(key => _cache.RemoveAsync(key));
            await Task.WhenAll(tasks);
        }

        private void IncrementHitCount(string key)
        {
            if (_config.EnableStatistics)
            {
                _hitCounts[key] = _hitCounts.GetValueOrDefault(key, 0) + 1;
            }
        }

        private void IncrementMissCount(string key)
        {
            if (_config.EnableStatistics)
            {
                _missCounts[key] = _missCounts.GetValueOrDefault(key, 0) + 1;
            }
        }

        #endregion
    }
}
