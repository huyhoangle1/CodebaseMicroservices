using CourseManager.Shared.DTOs.Notification;
using CourseManager.Shared.Repositories;

namespace CourseManager.Shared.Services
{
    /// <summary>
    /// Service quyền hạn với cache Redis để tối ưu hiệu suất
    /// </summary>
    public class CachedPermissionService : IPermissionService
    {
        private readonly IPermissionService _permissionService;
        private readonly IPermissionCacheService _cacheService;
        private readonly ILogger<CachedPermissionService> _logger;

        public CachedPermissionService(
            IPermissionService permissionService,
            IPermissionCacheService cacheService,
            ILogger<CachedPermissionService> logger)
        {
            _permissionService = permissionService;
            _cacheService = cacheService;
            _logger = logger;
        }

        #region Permission Management

        public async Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync()
        {
            return await _permissionService.GetAllPermissionsAsync();
        }

        public async Task<PermissionDto?> GetPermissionByIdAsync(int id)
        {
            return await _permissionService.GetPermissionByIdAsync(id);
        }

        public async Task<PermissionDto> CreatePermissionAsync(CreatePermissionRequest request)
        {
            var result = await _permissionService.CreatePermissionAsync(request);
            
            // Invalidate cache khi tạo permission mới
            await _cacheService.InvalidatePermissionCacheAsync(result.Id);
            
            return result;
        }

        public async Task<PermissionDto> UpdatePermissionAsync(int id, UpdatePermissionRequest request)
        {
            var result = await _permissionService.UpdatePermissionAsync(id, request);
            
            // Invalidate cache khi cập nhật permission
            await _cacheService.InvalidatePermissionCacheAsync(id);
            
            return result;
        }

        public async Task<bool> DeletePermissionAsync(int id)
        {
            var result = await _permissionService.DeletePermissionAsync(id);
            
            // Invalidate cache khi xóa permission
            await _cacheService.InvalidatePermissionCacheAsync(id);
            
            return result;
        }

        public async Task<bool> ActivatePermissionAsync(int id)
        {
            var result = await _permissionService.ActivatePermissionAsync(id);
            
            // Invalidate cache khi thay đổi trạng thái permission
            await _cacheService.InvalidatePermissionCacheAsync(id);
            
            return result;
        }

        public async Task<bool> DeactivatePermissionAsync(int id)
        {
            var result = await _permissionService.DeactivatePermissionAsync(id);
            
            // Invalidate cache khi thay đổi trạng thái permission
            await _cacheService.InvalidatePermissionCacheAsync(id);
            
            return result;
        }

        #endregion

        #region User Permission Management

        public async Task<IEnumerable<PermissionDto>> GetUserPermissionsAsync(int userId)
        {
            // Thử lấy từ cache trước
            var cachedPermissions = await _cacheService.GetUserPermissionsAsync(userId);
            if (cachedPermissions.Any())
            {
                _logger.LogDebug("Retrieved {Count} permissions for user {UserId} from cache", 
                    cachedPermissions.Count, userId);
                
                // Convert cached permission strings back to PermissionDto
                return await ConvertCachedPermissionsToDtosAsync(cachedPermissions);
            }

            // Nếu không có trong cache, lấy từ database
            var permissions = await _permissionService.GetUserPermissionsAsync(userId);
            
            // Cache kết quả
            var permissionStrings = permissions.Select(p => $"{p.Resource}:{p.Action}").ToList();
            await _cacheService.SetUserPermissionsAsync(userId, permissionStrings);
            
            _logger.LogDebug("Retrieved {Count} permissions for user {UserId} from database and cached", 
                permissions.Count(), userId);
            
            return permissions;
        }

        public async Task<bool> AssignPermissionToUserAsync(int userId, int permissionId, DateTime? expiresAt = null)
        {
            var result = await _permissionService.AssignPermissionToUserAsync(userId, permissionId, expiresAt);
            
            // Invalidate user cache khi gán permission
            if (result)
            {
                await _cacheService.InvalidateUserCacheAsync(userId);
            }
            
            return result;
        }

        public async Task<bool> RevokePermissionFromUserAsync(int userId, int permissionId)
        {
            var result = await _permissionService.RevokePermissionFromUserAsync(userId, permissionId);
            
            // Invalidate user cache khi thu hồi permission
            if (result)
            {
                await _cacheService.InvalidateUserCacheAsync(userId);
            }
            
            return result;
        }

        public async Task<bool> UserHasPermissionAsync(int userId, string resource, string action)
        {
            // Thử kiểm tra từ cache trước
            var hasPermission = await _cacheService.UserHasPermissionAsync(userId, resource, action);
            if (hasPermission)
            {
                _logger.LogDebug("User {UserId} has permission {Resource}:{Action} (from cache)", 
                    userId, resource, action);
                return true;
            }

            // Nếu không có trong cache, kiểm tra từ database
            hasPermission = await _permissionService.UserHasPermissionAsync(userId, resource, action);
            
            // Nếu có permission, cache lại toàn bộ permissions của user
            if (hasPermission)
            {
                await RefreshUserPermissionsCacheAsync(userId);
            }
            
            _logger.LogDebug("User {UserId} has permission {Resource}:{Action} (from database): {HasPermission}", 
                userId, resource, action, hasPermission);
            
            return hasPermission;
        }

        public async Task<Dictionary<string, List<string>>> GetUserPermissionMatrixAsync(int userId)
        {
            // Thử lấy từ cache trước
            var cachedMatrix = await _cacheService.GetUserPermissionMatrixAsync(userId);
            if (cachedMatrix.Any())
            {
                _logger.LogDebug("Retrieved permission matrix for user {UserId} from cache", userId);
                return cachedMatrix;
            }

            // Nếu không có trong cache, lấy từ database
            var matrix = await _permissionService.GetUserPermissionMatrixAsync(userId);
            
            // Cache kết quả
            await _cacheService.SetUserPermissionMatrixAsync(userId, matrix);
            
            _logger.LogDebug("Retrieved permission matrix for user {UserId} from database and cached", userId);
            
            return matrix;
        }

        #endregion

        #region Role Permission Management

        public async Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(int roleId)
        {
            // Thử lấy từ cache trước
            var cachedPermissions = await _cacheService.GetRolePermissionsAsync(roleId);
            if (cachedPermissions.Any())
            {
                _logger.LogDebug("Retrieved {Count} permissions for role {RoleId} from cache", 
                    cachedPermissions.Count, roleId);
                
                return await ConvertCachedPermissionsToDtosAsync(cachedPermissions);
            }

            // Nếu không có trong cache, lấy từ database
            var permissions = await _permissionService.GetRolePermissionsAsync(roleId);
            
            // Cache kết quả
            var permissionStrings = permissions.Select(p => $"{p.Resource}:{p.Action}").ToList();
            await _cacheService.SetRolePermissionsAsync(roleId, permissionStrings);
            
            _logger.LogDebug("Retrieved {Count} permissions for role {RoleId} from database and cached", 
                permissions.Count(), roleId);
            
            return permissions;
        }

        public async Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId)
        {
            var result = await _permissionService.AssignPermissionToRoleAsync(roleId, permissionId);
            
            // Invalidate role cache khi gán permission
            if (result)
            {
                await _cacheService.InvalidateRoleCacheAsync(roleId);
            }
            
            return result;
        }

        public async Task<bool> RevokePermissionFromRoleAsync(int roleId, int permissionId)
        {
            var result = await _permissionService.RevokePermissionFromRoleAsync(roleId, permissionId);
            
            // Invalidate role cache khi thu hồi permission
            if (result)
            {
                await _cacheService.InvalidateRoleCacheAsync(roleId);
            }
            
            return result;
        }

        public async Task<bool> RoleHasPermissionAsync(int roleId, string resource, string action)
        {
            // Thử kiểm tra từ cache trước
            var hasPermission = await _cacheService.RoleHasPermissionAsync(roleId, resource, action);
            if (hasPermission)
            {
                _logger.LogDebug("Role {RoleId} has permission {Resource}:{Action} (from cache)", 
                    roleId, resource, action);
                return true;
            }

            // Nếu không có trong cache, kiểm tra từ database
            hasPermission = await _permissionService.RoleHasPermissionAsync(roleId, resource, action);
            
            // Nếu có permission, cache lại toàn bộ permissions của role
            if (hasPermission)
            {
                await RefreshRolePermissionsCacheAsync(roleId);
            }
            
            _logger.LogDebug("Role {RoleId} has permission {Resource}:{Action} (from database): {HasPermission}", 
                roleId, resource, action, hasPermission);
            
            return hasPermission;
        }

        #endregion

        #region Permission Checking

        public async Task<bool> CheckPermissionAsync(int userId, string resource, string action)
        {
            return await UserHasPermissionAsync(userId, resource, action);
        }

        public async Task<IEnumerable<string>> GetUserPermissionsByResourceAsync(int userId, string resource)
        {
            var matrix = await GetUserPermissionMatrixAsync(userId);
            return matrix.GetValueOrDefault(resource, new List<string>());
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissionsByResourceAsync(string resource)
        {
            return await _permissionService.GetPermissionsByResourceAsync(resource);
        }

        public async Task<IEnumerable<PermissionDto>> GetPermissionsByModuleAsync(string module)
        {
            return await _permissionService.GetPermissionsByModuleAsync(module);
        }

        #endregion

        #region Bulk Operations

        public async Task<bool> AssignMultiplePermissionsToUserAsync(int userId, List<int> permissionIds, DateTime? expiresAt = null)
        {
            var result = await _permissionService.AssignMultiplePermissionsToUserAsync(userId, permissionIds, expiresAt);
            
            // Invalidate user cache khi gán nhiều permissions
            if (result)
            {
                await _cacheService.InvalidateUserCacheAsync(userId);
            }
            
            return result;
        }

        public async Task<bool> AssignMultiplePermissionsToRoleAsync(int roleId, List<int> permissionIds)
        {
            var result = await _permissionService.AssignMultiplePermissionsToRoleAsync(roleId, permissionIds);
            
            // Invalidate role cache khi gán nhiều permissions
            if (result)
            {
                await _cacheService.InvalidateRoleCacheAsync(roleId);
            }
            
            return result;
        }

        public async Task<bool> RevokeAllUserPermissionsAsync(int userId)
        {
            var result = await _permissionService.RevokeAllUserPermissionsAsync(userId);
            
            // Invalidate user cache khi thu hồi tất cả permissions
            if (result)
            {
                await _cacheService.InvalidateUserCacheAsync(userId);
            }
            
            return result;
        }

        public async Task<bool> RevokeAllRolePermissionsAsync(int roleId)
        {
            var result = await _permissionService.RevokeAllRolePermissionsAsync(roleId);
            
            // Invalidate role cache khi thu hồi tất cả permissions
            if (result)
            {
                await _cacheService.InvalidateRoleCacheAsync(roleId);
            }
            
            return result;
        }

        #endregion

        #region Private Methods

        private async Task<List<PermissionDto>> ConvertCachedPermissionsToDtosAsync(List<string> cachedPermissions)
        {
            var permissions = new List<PermissionDto>();
            
            foreach (var permissionString in cachedPermissions)
            {
                var parts = permissionString.Split(':');
                if (parts.Length == 2)
                {
                    permissions.Add(new PermissionDto
                    {
                        Resource = parts[0],
                        Action = parts[1],
                        Name = permissionString
                    });
                }
            }
            
            return permissions;
        }

        private async Task RefreshUserPermissionsCacheAsync(int userId)
        {
            try
            {
                var permissions = await _permissionService.GetUserPermissionsAsync(userId);
                var permissionStrings = permissions.Select(p => $"{p.Resource}:{p.Action}").ToList();
                await _cacheService.SetUserPermissionsAsync(userId, permissionStrings);
                
                _logger.LogDebug("Refreshed permissions cache for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing permissions cache for user {UserId}", userId);
            }
        }

        private async Task RefreshRolePermissionsCacheAsync(int roleId)
        {
            try
            {
                var permissions = await _permissionService.GetRolePermissionsAsync(roleId);
                var permissionStrings = permissions.Select(p => $"{p.Resource}:{p.Action}").ToList();
                await _cacheService.SetRolePermissionsAsync(roleId, permissionStrings);
                
                _logger.LogDebug("Refreshed permissions cache for role {RoleId}", roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing permissions cache for role {RoleId}", roleId);
            }
        }

        #endregion
    }
}
