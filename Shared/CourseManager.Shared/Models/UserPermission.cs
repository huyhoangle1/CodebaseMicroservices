using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseManager.Shared.Models
{
    /// <summary>
    /// Bảng liên kết User và Permission (quyền trực tiếp)
    /// </summary>
    [Table("UserPermissions")]
    public class UserPermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public int? AssignedBy { get; set; } // User gán permission

        public DateTime? ExpiresAt { get; set; } // Thời hạn permission

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; } = null!;

        [ForeignKey("AssignedBy")]
        public virtual User? AssignedByUser { get; set; }
    }
}
