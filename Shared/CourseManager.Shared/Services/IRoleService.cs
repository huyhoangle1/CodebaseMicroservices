using CourseManager.Shared.DTOs;
using CourseManager.Shared.Models;

namespace CourseManager.Shared.Services
{
    public interface IRoleService
    {
        // Role Management
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RoleDto?> GetRoleByIdAsync(int id);
        Task<RoleDto> CreateRoleAsync(CreateRoleRequest request);
        Task<RoleDto> UpdateRoleAsync(int id, UpdateRoleRequest request);
        Task<bool> DeleteRoleAsync(int id);
        Task<bool> ActivateRoleAsync(int id);
        Task<bool> DeactivateRoleAsync(int id);

        // User Role Management
        Task<IEnumerable<RoleDto>> GetUserRolesAsync(int userId);
        Task<IEnumerable<UserRoleDto>> GetUserRoleAssignmentsAsync(int userId);
        Task<bool> AssignRoleToUserAsync(AssignRoleRequest request);
        Task<bool> RevokeRoleFromUserAsync(int userId, int roleId);
        Task<bool> UserHasRoleAsync(int userId, string roleName);
        Task<Dictionary<string, List<string>>> GetUserRoleMatrixAsync(int userId);

        // Role Permission Management
        Task<IEnumerable<PermissionDto>> GetRolePermissionsAsync(int roleId);
        Task<bool> AssignPermissionToRoleAsync(int roleId, int permissionId);
        Task<bool> RevokePermissionFromRoleAsync(int roleId, int permissionId);
        Task<bool> RoleHasPermissionAsync(int roleId, string resource, string action);

        // Role Menu Management
        Task<IEnumerable<MenuDto>> GetRoleMenusAsync(int roleId);
        Task<bool> AssignMenuToRoleAsync(int roleId, int menuId);
        Task<bool> RevokeMenuFromRoleAsync(int roleId, int menuId);
        Task<bool> RoleCanAccessMenuAsync(int roleId, int menuId);

        // Bulk Operations
        Task<bool> AssignMultipleRolesToUserAsync(int userId, List<int> roleIds, DateTime? expiresAt = null);
        Task<bool> AssignMultiplePermissionsToRoleAsync(int roleId, List<int> permissionIds);
        Task<bool> AssignMultipleMenusToRoleAsync(int roleId, List<int> menuIds);
        Task<bool> RevokeAllUserRolesAsync(int userId);
        Task<bool> RevokeAllRolePermissionsAsync(int roleId);
        Task<bool> RevokeAllRoleMenusAsync(int roleId);

        // System Roles
        Task<IEnumerable<RoleDto>> GetSystemRolesAsync();
        Task<bool> IsSystemRoleAsync(int roleId);
        Task<bool> CanDeleteRoleAsync(int roleId);
    }
}
