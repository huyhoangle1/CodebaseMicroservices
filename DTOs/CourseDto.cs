namespace CourseManager.API.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string? ImageUrl { get; set; }
        public string? Instructor { get; set; }
        public string? VideoUrl { get; set; }
        public int Duration { get; set; }
        public string Level { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal DiscountPercentage { get; set; }
        public bool IsOnSale { get; set; }
    }

    public class CreateCourseRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string? ImageUrl { get; set; }
        public string? Instructor { get; set; }
        public string? VideoUrl { get; set; }
        public int Duration { get; set; }
        public string Level { get; set; } = "Beginner";
    }

    public class UpdateCourseRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string? ImageUrl { get; set; }
        public string? Instructor { get; set; }
        public string? VideoUrl { get; set; }
        public int Duration { get; set; }
        public string Level { get; set; } = "Beginner";
        public bool IsActive { get; set; } = true;
    }
}
