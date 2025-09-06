using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseManager.Shared.Models
{
    /// <summary>
    /// Bảng liên kết Role và Permission
    /// </summary>
    [Table("RolePermissions")]
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public int? AssignedBy { get; set; } // User gán permission

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; } = null!;

        [ForeignKey("AssignedBy")]
        public virtual User? AssignedByUser { get; set; }
    }
}
