using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseManager.Shared.Models
{
    /// <summary>
    /// Bảng vai trò/nhóm quyền
    /// </summary>
    [Table("Roles")]
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [StringLength(20)]
        public string? Color { get; set; } // Màu hiển thị cho role

        [StringLength(50)]
        public string? Icon { get; set; } // Icon cho role

        public int Priority { get; set; } = 0; // Thứ tự ưu tiên

        public bool IsSystemRole { get; set; } = false; // Role hệ thống không thể xóa

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public virtual ICollection<MenuRole> MenuRoles { get; set; } = new List<MenuRole>();
    }
}
