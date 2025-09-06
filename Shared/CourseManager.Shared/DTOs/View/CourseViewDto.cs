namespace CourseManager.Shared.DTOs.View
{
    /// <summary>
    /// DTO xem chi tiết khóa học
    /// </summary>
    public class CourseViewDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Instructor { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int EnrolledCount { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
    }

    /// <summary>
    /// DTO xem danh sách khóa học (rút gọn)
    /// </summary>
    public class CourseListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Instructor { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public string? ImageUrl { get; set; }
        public int EnrolledCount { get; set; }
        public double Rating { get; set; }
    }

    /// <summary>
    /// DTO xem khóa học với thông tin bổ sung
    /// </summary>
    public class CourseDetailDto : CourseViewDto
    {
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Prerequisites { get; set; } = new List<string>();
        public List<string> LearningOutcomes { get; set; } = new List<string>();
        public List<CourseReviewDto> Reviews { get; set; } = new List<CourseReviewDto>();
        public List<CourseModuleDto> Modules { get; set; } = new List<CourseModuleDto>();
    }

    /// <summary>
    /// DTO đánh giá khóa học
    /// </summary>
    public class CourseReviewDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO module khóa học
    /// </summary>
    public class CourseModuleDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Duration { get; set; }
        public int SortOrder { get; set; }
        public List<CourseLessonDto> Lessons { get; set; } = new List<CourseLessonDto>();
    }

    /// <summary>
    /// DTO bài học
    /// </summary>
    public class CourseLessonDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? VideoUrl { get; set; }
        public int Duration { get; set; }
        public int SortOrder { get; set; }
        public bool IsFree { get; set; }
    }
}
