using CourseManager.API.Models;

namespace CourseManager.API.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int id);
        Task<Order?> GetOrderByNumberAsync(string orderNumber);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
        Task<Order> CreateOrderAsync(Order order, List<OrderItem> orderItems);
        Task<Order> UpdateOrderStatusAsync(int id, string status);
        Task<Order> UpdatePaymentStatusAsync(int id, string paymentStatus, string? paymentReference = null);
        Task<bool> DeleteOrderAsync(int id);
    }
}

