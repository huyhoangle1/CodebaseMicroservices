using CourseManager.Shared.DTOs;
using CourseManager.Shared.Models;

namespace CourseManager.Shared.Services
{
    public interface IMenuService
    {
        // Menu Management
        Task<IEnumerable<MenuDto>> GetAllMenusAsync();
        Task<MenuDto?> GetMenuByIdAsync(int id);
        Task<MenuDto> CreateMenuAsync(CreateMenuRequest request);
        Task<MenuDto> UpdateMenuAsync(int id, UpdateMenuRequest request);
        Task<bool> DeleteMenuAsync(int id);
        Task<bool> ActivateMenuAsync(int id);
        Task<bool> DeactivateMenuAsync(int id);

        // Menu Tree Management
        Task<IEnumerable<MenuTreeDto>> GetMenuTreeAsync();
        Task<IEnumerable<MenuTreeDto>> GetMenuTreeByUserAsync(int userId);
        Task<IEnumerable<MenuTreeDto>> GetMenuTreeByRoleAsync(int roleId);
        Task<IEnumerable<MenuDto>> GetParentMenusAsync();
        Task<IEnumerable<MenuDto>> GetChildMenusAsync(int parentId);

        // User Menu Management
        Task<IEnumerable<MenuDto>> GetUserMenusAsync(int userId);
        Task<IEnumerable<UserMenuDto>> GetUserMenuTreeAsync(int userId);
        Task<bool> UserCanAccessMenuAsync(int userId, int menuId);
        Task<bool> UserCanAccessMenuByUrlAsync(int userId, string url);
        Task<Dictionary<string, List<Menu>>> GetUserMenuMatrixAsync(int userId);

        // Role Menu Management
        Task<IEnumerable<MenuDto>> GetRoleMenusAsync(int roleId);
        Task<bool> AssignMenuToRoleAsync(int roleId, int menuId);
        Task<bool> RevokeMenuFromRoleAsync(int roleId, int menuId);
        Task<bool> RoleCanAccessMenuAsync(int roleId, int menuId);

        // Menu by Module
        Task<IEnumerable<MenuDto>> GetMenusByModuleAsync(string module);
        Task<IEnumerable<MenuTreeDto>> GetMenuTreeByModuleAsync(string module);

        // Bulk Operations
        Task<bool> AssignMultipleMenusToRoleAsync(int roleId, List<int> menuIds);
        Task<bool> RevokeAllRoleMenusAsync(int roleId);
        Task<bool> UpdateMenuOrderAsync(List<MenuOrderRequest> menuOrders);

        // Menu Visibility
        Task<IEnumerable<MenuDto>> GetVisibleMenusAsync();
        Task<IEnumerable<MenuDto>> GetVisibleMenusByUserAsync(int userId);
        Task<bool> ToggleMenuVisibilityAsync(int menuId);
    }

    public class MenuOrderRequest
    {
        public int MenuId { get; set; }
        public int SortOrder { get; set; }
        public int? ParentId { get; set; }
    }
}
