using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseManager.Shared.Services;
using CourseManager.Shared.DTOs;
using CourseManager.Shared.Models;
using CourseManager.Shared.Repositories;
using CourseManager.Shared.Controllers;
using System.Linq.Expressions;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : BaseController
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;

        public OrdersController(
            IOrderRepository orderRepository, 
            IOrderService orderService,
            IMapper mapper, 
            ILogger<OrdersController> logger) 
            : base(mapper, logger)
        {
            _orderRepository = orderRepository;
            _orderService = orderService;
        }

        /// <summary>
        /// Lấy tất cả đơn hàng (chỉ Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
        {
            return await GetAllAsync<Order, OrderDto>(_orderRepository, "orders.read");
        }

        /// <summary>
        /// Lấy đơn hàng theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(id, "User", "OrderItems", "OrderItems.Course");
                if (order == null)
                {
                    return HandleBadRequestResult($"Order with ID {id} not found");
                }

                // Kiểm tra quyền truy cập - chỉ admin hoặc chủ đơn hàng
                var currentUserId = GetCurrentUserId();
                if (!IsAdmin() && order.UserId != currentUserId)
                {
                    return HandleForbiddenResult("You don't have permission to view this order");
                }

                var orderDto = _mapper.Map<OrderDto>(order);
                return HandleResult(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting order with ID {OrderId}", id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy đơn hàng theo số đơn hàng
        /// </summary>
        [HttpGet("number/{orderNumber}")]
        public async Task<ActionResult<OrderDto>> GetOrderByNumber(string orderNumber)
        {
            try
            {
                var order = await _orderRepository.GetOrderByNumberAsync(orderNumber);
                if (order == null)
                {
                    return HandleBadRequestResult($"Order with number {orderNumber} not found");
                }

                // Kiểm tra quyền truy cập
                var currentUserId = GetCurrentUserId();
                if (!IsAdmin() && order.UserId != currentUserId)
                {
                    return HandleForbiddenResult("You don't have permission to view this order");
                }

                var orderDto = _mapper.Map<OrderDto>(order);
                return HandleResult(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting order with number {OrderNumber}", orderNumber);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy đơn hàng của user hiện tại
        /// </summary>
        [HttpGet("my-orders")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return HandleUnauthorizedResult("User not authenticated");
                }

                var orders = await _orderRepository.GetOrdersByUserIdAsync(currentUserId);
                var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);
                return HandleResult(orderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting orders for user {UserId}", GetCurrentUserId());
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Tạo đơn hàng mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return HandleUnauthorizedResult("User not authenticated");
                }

                // Set user ID from token
                request.UserId = currentUserId;

                var order = _mapper.Map<Order>(request);
                var orderItems = _mapper.Map<List<OrderItem>>(request.OrderItems);

                var createdOrder = await _orderService.CreateOrderAsync(order, orderItems);
                var orderDto = _mapper.Map<OrderDto>(createdOrder);

                return HandleCreatedResult(orderDto, nameof(GetOrderById), new { id = createdOrder.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order");
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng (chỉ Admin)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderDto>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var order = await _orderService.UpdateOrderStatusAsync(id, request.Status);
                var orderDto = _mapper.Map<OrderDto>(order);
                return HandleResult(orderDto);
            }
            catch (KeyNotFoundException)
            {
                return HandleBadRequestResult($"Order with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating order status for order {OrderId}", id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Cập nhật trạng thái thanh toán
        /// </summary>
        [HttpPut("{id}/payment")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderDto>> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusRequest request)
        {
            try
            {
                var order = await _orderService.UpdatePaymentStatusAsync(id, request.PaymentStatus, request.PaymentReference);
                var orderDto = _mapper.Map<OrderDto>(order);
                return HandleResult(orderDto);
            }
            catch (KeyNotFoundException)
            {
                return HandleBadRequestResult($"Order with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating payment status for order {OrderId}", id);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Xóa đơn hàng (chỉ Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            return await DeleteAsync<Order>(_orderRepository, id);
        }

        /// <summary>
        /// Lấy đơn hàng với phân trang
        /// </summary>
        [HttpGet("paged")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PagedResult<OrderDto>>> GetPagedOrders(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? paymentStatus = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                Expression<Func<Order, bool>>? predicate = null;

                if (!string.IsNullOrWhiteSpace(status))
                {
                    predicate = o => o.Status.ToLower() == status.ToLower();
                }

                if (!string.IsNullOrWhiteSpace(paymentStatus))
                {
                    var paymentPredicate = o => o.PaymentStatus.ToLower() == paymentStatus.ToLower();
                    predicate = predicate == null ? paymentPredicate : 
                               Expression.Lambda<Func<Order, bool>>(
                                   Expression.AndAlso(predicate.Body, paymentPredicate.Body), 
                                   predicate.Parameters);
                }

                if (fromDate.HasValue)
                {
                    var fromDatePredicate = o => o.CreatedAt >= fromDate.Value;
                    predicate = predicate == null ? fromDatePredicate : 
                               Expression.Lambda<Func<Order, bool>>(
                                   Expression.AndAlso(predicate.Body, fromDatePredicate.Body), 
                                   predicate.Parameters);
                }

                if (toDate.HasValue)
                {
                    var toDatePredicate = o => o.CreatedAt <= toDate.Value;
                    predicate = predicate == null ? toDatePredicate : 
                               Expression.Lambda<Func<Order, bool>>(
                                   Expression.AndAlso(predicate.Body, toDatePredicate.Body), 
                                   predicate.Parameters);
                }

                if (predicate != null)
                {
                    return await GetPagedAsync<Order, OrderDto>(_orderRepository, pageNumber, pageSize, predicate);
                }
                else
                {
                    return await GetPagedAsync<Order, OrderDto>(_orderRepository, pageNumber, pageSize);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged orders");
                return HandleInternalServerErrorResult();
            }
        }
    }
}
