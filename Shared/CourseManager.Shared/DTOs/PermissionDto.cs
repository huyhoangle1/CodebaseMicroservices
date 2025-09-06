namespace CourseManager.Shared.DTOs
{
    public class PermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Module { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreatePermissionRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Module { get; set; }
    }

    public class UpdatePermissionRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Module { get; set; }
        public bool IsActive { get; set; }
    }
}
