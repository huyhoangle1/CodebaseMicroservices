using CourseManager.Shared.Models;
using System.Linq.Expressions;

namespace CourseManager.Shared.Repositories
{
    public interface IRoleRepository : IBaseRepository<Role>
    {
        Task<IEnumerable<Role>> GetUserRolesAsync(int userId);
        Task<IEnumerable<Role>> GetActiveRolesAsync();
        Task<Role?> GetRoleWithPermissionsAsync(int roleId);
        Task<Role?> GetRoleWithMenusAsync(int roleId);
        Task<bool> UserHasRoleAsync(int userId, string roleName);
        Task<IEnumerable<Role>> GetRolesByPermissionAsync(int permissionId);
        Task<IEnumerable<Role>> GetRolesByMenuAsync(int menuId);
        Task<Dictionary<string, List<string>>> GetUserRoleMatrixAsync(int userId);
    }
}
