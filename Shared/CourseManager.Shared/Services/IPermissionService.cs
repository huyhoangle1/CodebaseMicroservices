using CourseManager.Shared.DTOs;
using CourseManager.Shared.Models;

namespace CourseManager.Shared.Services
{
    public interface IPermissionService
    {
        // Permission Management
        Task<IEnumerable<PermissionDto>> GetAllPermissionsAsync();
        Task<PermissionDto?> GetPermissionByIdAsync(int id);
        Task<PermissionDto> CreatePermissionAsync(CreatePermissionRequest request);
        Task<PermissionDto> UpdatePermissionAsync(int id, UpdatePermissionRequest request);
        Task<bool> DeletePermissionAsync(int id);
        Task<bool> ActivatePermissionAsync(int id);
        Task<bool> DeactivatePermissionAsync(int id);

        // User Permission Management
        Task<IEnumerable<PermissionDto>> GetUserPermissionsAsync(int userId);
        Task<bool> AssignPermissionToUserAsync(int userId, int permissionId, DateTime? expiresAt = null);
        Task<bool> RevokePermissionFromUserAsync(int userId, int permissionId);
        Task<bool> UserHasPermissionAsync(int userId, string resource, string action);
        Task<Dictionary<string, List<string>>> GetUserPermissionMatrixAsync(int userId);

        // Role Permission Management
        Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(int roleId);
        Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId);
        Task<bool> RevokePermissionFromRoleAsync(int roleId, int permissionId);
        Task<bool> RoleHasPermissionAsync(int roleId, string resource, string action);

        // Permission Checking
        Task<bool> CheckPermissionAsync(int userId, string resource, string action);
        Task<IEnumerable<string>> GetUserPermissionsByResourceAsync(int userId, string resource);
        Task<IEnumerable<PermissionDto>> GetPermissionsByResourceAsync(string resource);
        Task<IEnumerable<PermissionDto>> GetPermissionsByModuleAsync(string module);

        // Bulk Operations
        Task<bool> AssignMultiplePermissionsToUserAsync(int userId, List<int> permissionIds, DateTime? expiresAt = null);
        Task<bool> AssignMultiplePermissionsToRoleAsync(int roleId, List<int> permissionIds);
        Task<bool> RevokeAllUserPermissionsAsync(int userId);
        Task<bool> RevokeAllRolePermissionsAsync(int roleId);
    }
}
