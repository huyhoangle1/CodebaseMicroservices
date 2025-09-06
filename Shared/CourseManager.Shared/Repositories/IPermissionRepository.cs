using CourseManager.Shared.Models;
using System.Linq.Expressions;

namespace CourseManager.Shared.Repositories
{
    public interface IPermissionRepository : IBaseRepository<Permission>
    {
        Task<IEnumerable<Permission>> GetPermissionsByResourceAsync(string resource);
        Task<IEnumerable<Permission>> GetPermissionsByModuleAsync(string module);
        Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId);
        Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId);
        Task<bool> UserHasPermissionAsync(int userId, string resource, string action);
        Task<bool> RoleHasPermissionAsync(int roleId, string resource, string action);
        Task<IEnumerable<Permission>> GetPermissionsByUserAndResourceAsync(int userId, string resource);
        Task<Dictionary<string, List<string>>> GetUserPermissionMatrixAsync(int userId);
    }
}
