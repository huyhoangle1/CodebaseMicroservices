namespace CourseManager.API.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string? PaymentReference { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string? ImageUrl { get; set; }
        public string? Instructor { get; set; }
    }

    public class CreateOrderRequest
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? Note { get; set; }
        public List<CreateOrderItemRequest> OrderItems { get; set; } = new();
    }

    public class CreateOrderItemRequest
    {
        public int CourseId { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class UpdatePaymentStatusRequest
    {
        public string PaymentStatus { get; set; } = string.Empty;
        public string? PaymentReference { get; set; }
    }
}
