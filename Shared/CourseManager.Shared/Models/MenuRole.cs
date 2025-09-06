using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseManager.Shared.Models
{
    /// <summary>
    /// Bảng liên kết Menu và Role
    /// </summary>
    [Table("MenuRoles")]
    public class MenuRole
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MenuId { get; set; }

        [Required]
        public int RoleId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public int? AssignedBy { get; set; } // User gán menu cho role

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("MenuId")]
        public virtual Menu Menu { get; set; } = null!;

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey("AssignedBy")]
        public virtual User? AssignedByUser { get; set; }
    }
}
