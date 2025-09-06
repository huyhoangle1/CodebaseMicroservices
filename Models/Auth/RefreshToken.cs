using System.ComponentModel.DataAnnotations;

namespace CourseManager.API.Models.Auth
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string JwtId { get; set; } = string.Empty;

        public bool IsUsed { get; set; } = false;

        public bool IsRevoked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}
