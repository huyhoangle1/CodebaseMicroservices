namespace CourseManager.Shared.DTOs
{
    public class MenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int SortOrder { get; set; }
        public string? Permission { get; set; }
        public string? Component { get; set; }
        public string? Module { get; set; }
        public bool IsVisible { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<MenuDto> Children { get; set; } = new List<MenuDto>();
    }

    public class CreateMenuRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public string? Permission { get; set; }
        public string? Component { get; set; }
        public string? Module { get; set; }
        public bool IsVisible { get; set; } = true;
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class UpdateMenuRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public string? Permission { get; set; }
        public string? Component { get; set; }
        public string? Module { get; set; }
        public bool IsVisible { get; set; }
        public bool IsActive { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class UserMenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public string? Component { get; set; }
        public string? Module { get; set; }
        public List<UserMenuDto> Children { get; set; } = new List<UserMenuDto>();
    }

    public class MenuTreeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public List<MenuTreeDto> Children { get; set; } = new List<MenuTreeDto>();
    }
}
