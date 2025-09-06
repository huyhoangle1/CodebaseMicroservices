namespace CourseManager.Shared.DTOs
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public int Priority { get; set; }
        public bool IsSystemRole { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
        public List<MenuDto> Menus { get; set; } = new List<MenuDto>();
    }

    public class CreateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public int Priority { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();
        public List<int> MenuIds { get; set; } = new List<int>();
    }

    public class UpdateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public List<int> PermissionIds { get; set; } = new List<int>();
        public List<int> MenuIds { get; set; } = new List<int>();
    }

    public class AssignRoleRequest
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class UserRoleDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }
}
