using Microsoft.EntityFrameworkCore;
using CourseManager.API.Data;
using CourseManager.API.Models;

namespace CourseManager.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly CourseManagerDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(CourseManagerDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all orders");
                throw;
            }
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Course)
                    .FirstOrDefaultAsync(o => o.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting order with ID {OrderId}", id);
                throw;
            }
        }

        public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Course)
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting order with number {OrderNumber}", orderNumber);
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Course)
                    .Where(o => o.UserId == userId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting orders for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Order> CreateOrderAsync(Order order, List<OrderItem> orderItems)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Generate order number
                order.OrderNumber = GenerateOrderNumber();
                order.CreatedAt = DateTime.UtcNow;
                order.UpdatedAt = DateTime.UtcNow;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Add order items
                foreach (var item in orderItems)
                {
                    item.OrderId = order.Id;
                    _context.OrderItems.Add(item);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Order created successfully with ID {OrderId} and number {OrderNumber}", 
                    order.Id, order.OrderNumber);
                
                return order;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while creating order");
                throw;
            }
        }

        public async Task<Order> UpdateOrderStatusAsync(int id, string status)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    throw new KeyNotFoundException($"Order with ID {id} not found");
                }

                order.Status = status;
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Order status updated to {Status} for order ID {OrderId}", status, id);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating order status for ID {OrderId}", id);
                throw;
            }
        }

        public async Task<Order> UpdatePaymentStatusAsync(int id, string paymentStatus, string? paymentReference = null)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    throw new KeyNotFoundException($"Order with ID {id} not found");
                }

                order.PaymentStatus = paymentStatus;
                if (!string.IsNullOrEmpty(paymentReference))
                {
                    order.PaymentReference = paymentReference;
                }
                order.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment status updated to {PaymentStatus} for order ID {OrderId}", paymentStatus, id);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating payment status for order ID {OrderId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return false;
                }

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Order deleted successfully with ID {OrderId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting order with ID {OrderId}", id);
                throw;
            }
        }

        private string GenerateOrderNumber()
        {
            return $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
        }
    }
}

