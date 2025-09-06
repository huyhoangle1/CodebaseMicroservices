using CourseManager.Shared.Models;
using System.Linq.Expressions;

namespace CourseManager.Shared.Repositories
{
    public interface IMenuRepository : IBaseRepository<Menu>
    {
        Task<IEnumerable<Menu>> GetUserMenusAsync(int userId);
        Task<IEnumerable<Menu>> GetRoleMenusAsync(int roleId);
        Task<IEnumerable<Menu>> GetMenusByModuleAsync(string module);
        Task<IEnumerable<Menu>> GetMenuTreeAsync();
        Task<IEnumerable<Menu>> GetMenuTreeByUserAsync(int userId);
        Task<IEnumerable<Menu>> GetMenuTreeByRoleAsync(int roleId);
        Task<IEnumerable<Menu>> GetParentMenusAsync();
        Task<IEnumerable<Menu>> GetChildMenusAsync(int parentId);
        Task<bool> UserCanAccessMenuAsync(int userId, int menuId);
        Task<bool> RoleCanAccessMenuAsync(int roleId, int menuId);
        Task<Dictionary<string, List<Menu>>> GetUserMenuMatrixAsync(int userId);
    }
}
