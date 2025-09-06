using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CourseManager.API.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [StringLength(200)]
        public string CourseTitle { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [StringLength(200)]
        public string? ImageUrl { get; set; }

        [StringLength(100)]
        public string? Instructor { get; set; }

        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;
    }
}

