using System.ComponentModel.DataAnnotations;

namespace CourseManager.Shared.DTOs.Update
{
    /// <summary>
    /// DTO cập nhật khóa học
    /// </summary>
    public class CourseUpdateDto
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

        public bool IsActive { get; set; } = true;
        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Prerequisites { get; set; } = new List<string>();
        public List<string> LearningOutcomes { get; set; } = new List<string>();
    }

    /// <summary>
    /// DTO cập nhật danh mục
    /// </summary>
    public class CategoryUpdateDto
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

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO cập nhật đơn hàng
    /// </summary>
    public class OrderUpdateDto
    {
        [StringLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự")]
        public string? Status { get; set; }

        [StringLength(20, ErrorMessage = "Trạng thái thanh toán không được vượt quá 20 ký tự")]
        public string? PaymentStatus { get; set; }

        [StringLength(100, ErrorMessage = "Mã tham chiếu thanh toán không được vượt quá 100 ký tự")]
        public string? PaymentReference { get; set; }

        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO cập nhật người dùng
    /// </summary>
    public class UserUpdateDto
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

        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string? PhoneNumber { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        public string? Address { get; set; }

        [StringLength(20, ErrorMessage = "Vai trò không được vượt quá 20 ký tự")]
        public string Role { get; set; } = "User";

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO cập nhật trạng thái đơn hàng
    /// </summary>
    public class OrderStatusUpdateDto
    {
        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [StringLength(20, ErrorMessage = "Trạng thái không được vượt quá 20 ký tự")]
        public string Status { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO cập nhật trạng thái thanh toán
    /// </summary>
    public class PaymentStatusUpdateDto
    {
        [Required(ErrorMessage = "Trạng thái thanh toán là bắt buộc")]
        [StringLength(20, ErrorMessage = "Trạng thái thanh toán không được vượt quá 20 ký tự")]
        public string PaymentStatus { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Mã tham chiếu thanh toán không được vượt quá 100 ký tự")]
        public string? PaymentReference { get; set; }

        [StringLength(200, ErrorMessage = "Ghi chú không được vượt quá 200 ký tự")]
        public string? Notes { get; set; }
    }
}
