using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseManager.Shared.Models
{
    /// <summary>
    /// Bảng quyền hạn chi tiết
    /// </summary>
    [Table("Permissions")]
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Resource { get; set; } = string.Empty; // courses, orders, users, etc.

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // create, read, update, delete

        [StringLength(100)]
        public string? Module { get; set; } // Course Management, Order Management, etc.

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}
