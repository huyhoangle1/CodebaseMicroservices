using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseManager.Shared.Services;
using CourseManager.Shared.DTOs;
using CourseManager.Shared.Controllers;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : BaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;

        public PaymentsController(
            IPaymentService paymentService,
            IOrderService orderService,
            IMapper mapper, 
            ILogger<PaymentsController> logger) 
            : base(mapper, logger)
        {
            _paymentService = paymentService;
            _orderService = orderService;
        }

        /// <summary>
        /// Lấy danh sách phương thức thanh toán
        /// </summary>
        [HttpGet("methods")]
        [AllowAnonymous]
        public async Task<ActionResult<Dictionary<string, object>>> GetPaymentMethods()
        {
            try
            {
                var methods = await _paymentService.GetPaymentMethodsAsync();
                return HandleResult(methods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting payment methods");
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Tạo MoMo payment
        /// </summary>
        [HttpPost("momo/create")]
        public async Task<ActionResult<string>> CreateMoMoPayment([FromBody] CreateMoMoPaymentRequest request)
        {
            try
            {
                // Lấy thông tin đơn hàng
                var order = await _orderService.GetOrderByIdAsync(request.OrderId);
                if (order == null)
                {
                    return HandleBadRequestResult($"Order with ID {request.OrderId} not found");
                }

                // Kiểm tra quyền truy cập
                var currentUserId = GetCurrentUserId();
                if (!IsAdmin() && order.UserId != currentUserId)
                {
                    return HandleForbiddenResult("You don't have permission to create payment for this order");
                }

                var paymentUrl = await _paymentService.CreateMoMoPaymentAsync(
                    order, 
                    request.ReturnUrl, 
                    request.NotifyUrl);

                return HandleResult(paymentUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating MoMo payment for order {OrderId}", request.OrderId);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// MoMo payment callback
        /// </summary>
        [HttpPost("momo/callback")]
        [AllowAnonymous]
        public async Task<ActionResult> MoMoCallback([FromBody] MoMoCallbackRequest request)
        {
            try
            {
                _logger.LogInformation("Received MoMo callback for order {OrderId}", request.OrderId);

                // Verify payment
                var isValid = await _paymentService.VerifyMoMoPaymentAsync(
                    request.OrderId, 
                    request.RequestId, 
                    request.ResultCode);

                if (isValid)
                {
                    // Update order payment status
                    await _orderService.UpdatePaymentStatusAsync(
                        int.Parse(request.OrderId), 
                        "Completed", 
                        request.TransactionId);

                    _logger.LogInformation("Payment completed successfully for order {OrderId}", request.OrderId);
                }
                else
                {
                    // Update order payment status as failed
                    await _orderService.UpdatePaymentStatusAsync(
                        int.Parse(request.OrderId), 
                        "Failed", 
                        request.TransactionId);

                    _logger.LogWarning("Payment failed for order {OrderId}", request.OrderId);
                }

                return HandleResult(new { success = true, message = "Callback processed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing MoMo callback for order {OrderId}", request.OrderId);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Xử lý payment callback (generic)
        /// </summary>
        [HttpPost("callback")]
        [AllowAnonymous]
        public async Task<ActionResult> ProcessPaymentCallback([FromBody] PaymentCallbackRequest request)
        {
            try
            {
                _logger.LogInformation("Received payment callback for order {OrderId} with status {Status}", 
                    request.OrderId, request.Status);

                var result = await _paymentService.ProcessPaymentCallbackAsync(
                    request.OrderId, 
                    request.Status, 
                    request.TransactionId);

                if (result)
                {
                    // Update order status based on payment status
                    var orderId = int.Parse(request.OrderId);
                    await _orderService.UpdatePaymentStatusAsync(orderId, request.Status, request.TransactionId);

                    _logger.LogInformation("Payment callback processed successfully for order {OrderId}", request.OrderId);
                }

                return HandleResult(new { success = result, message = "Callback processed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing payment callback for order {OrderId}", request.OrderId);
                return HandleInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Lấy thông tin thanh toán của đơn hàng
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<PaymentInfoDto>> GetPaymentInfo(int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    return HandleBadRequestResult($"Order with ID {orderId} not found");
                }

                // Kiểm tra quyền truy cập
                var currentUserId = GetCurrentUserId();
                if (!IsAdmin() && order.UserId != currentUserId)
                {
                    return HandleForbiddenResult("You don't have permission to view payment info for this order");
                }

                var paymentInfo = new PaymentInfoDto
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    TotalAmount = order.TotalAmount,
                    PaymentMethod = order.PaymentMethod,
                    PaymentStatus = order.PaymentStatus,
                    PaymentReference = order.PaymentReference,
                    CreatedAt = order.CreatedAt
                };

                return HandleResult(paymentInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting payment info for order {OrderId}", orderId);
                return HandleInternalServerErrorResult();
            }
        }
    }

    // DTOs for payment requests
    public class CreateMoMoPaymentRequest
    {
        public int OrderId { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
        public string NotifyUrl { get; set; } = string.Empty;
    }

    public class MoMoCallbackRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public int ResultCode { get; set; }
        public string? TransactionId { get; set; }
    }

    public class PaymentCallbackRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
    }

    public class PaymentInfoDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string? PaymentReference { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
