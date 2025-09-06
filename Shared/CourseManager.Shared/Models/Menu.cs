using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseManager.Shared.Models
{
    /// <summary>
    /// Bảng menu hệ thống
    /// </summary>
    [Table("Menus")]
    public class Menu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Url { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; }

        [StringLength(20)]
        public string? Color { get; set; }

        public int? ParentId { get; set; } // Menu cha

        public int SortOrder { get; set; } = 0; // Thứ tự sắp xếp

        [StringLength(50)]
        public string? Permission { get; set; } // Quyền cần có để truy cập menu

        [StringLength(100)]
        public string? Component { get; set; } // Component frontend

        [StringLength(50)]
        public string? Module { get; set; } // Module chứa menu

        public bool IsVisible { get; set; } = true; // Hiển thị trong menu

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Menu? Parent { get; set; }
        public virtual ICollection<Menu> Children { get; set; } = new List<Menu>();
        public virtual ICollection<MenuRole> MenuRoles { get; set; } = new List<MenuRole>();
    }
}
