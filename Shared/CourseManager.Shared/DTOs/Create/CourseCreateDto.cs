using System.ComponentModel.DataAnnotations;

namespace CourseManager.Shared.DTOs.Create
{
    /// <summary>
    /// DTO tạo khóa học mới
    /// </summary>
    public class CourseCreateDto
    {
        [Required(ErrorMessage = "Tiêu đề khóa học là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giảng viên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên giảng viên không được vượt quá 100 ký tự")]
        public string Instructor { get; set; } = string.Empty;

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        [StringLength(50, ErrorMessage = "Danh mục không được vượt quá 50 ký tự")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cấp độ là bắt buộc")]
        [StringLength(20, ErrorMessage = "Cấp độ không được vượt quá 20 ký tự")]
        public string Level { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Thời lượng phải lớn hơn 0")]
        public int Duration { get; set; }

        [Url(ErrorMessage = "URL hình ảnh không hợp lệ")]
        public string? ImageUrl { get; set; }

        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Prerequisites { get; set; } = new List<string>();
        public List<string> LearningOutcomes { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO tạo danh mục mới
    /// </summary>
    public class CategoryCreateDto
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string? Description { get; set; }

        [StringLength(20, ErrorMessage = "Màu sắc không được vượt quá 20 ký tự")]
        public string? Color { get; set; }

        [StringLength(50, ErrorMessage = "Icon không được vượt quá 50 ký tự")]
        public string? Icon { get; set; }
    }

    /// <summary>
    /// DTO tạo đơn hàng mới
    /// </summary>
    public class OrderCreateDto
    {
        [Required(ErrorMessage = "Danh sách sản phẩm là bắt buộc")]
        [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất 1 sản phẩm")]
        public List<OrderItemCreateDto> OrderItems { get; set; } = new List<OrderItemCreateDto>();

        [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc")]
        [StringLength(20, ErrorMessage = "Phương thức thanh toán không được vượt quá 20 ký tự")]
        public string PaymentMethod { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
        public string? Notes { get; set; }

        public int UserId { get; set; }
    }

    /// <summary>
    /// DTO tạo chi tiết đơn hàng
    /// </summary>
    public class OrderItemCreateDto
    {
        [Required(ErrorMessage = "ID khóa học là bắt buộc")]
        public int CourseId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }
    }

    /// <summary>
    /// DTO tạo người dùng mới
    /// </summary>
    public class UserCreateDto
    {
        [Required(ErrorMessage = "Tên là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên không được vượt quá 50 ký tự")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ là bắt buộc")]
        [StringLength(50, ErrorMessage = "Họ không được vượt quá 50 ký tự")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có từ 6-100 ký tự")]
        public string Password { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string? PhoneNumber { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        public string? Address { get; set; }

        [StringLength(20, ErrorMessage = "Vai trò không được vượt quá 20 ký tự")]
        public string Role { get; set; } = "User";
    }
}
