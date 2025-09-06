using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CourseManager.API.Models;

namespace CourseManager.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;
        private readonly HttpClient _httpClient;

        public PaymentService(IConfiguration configuration, ILogger<PaymentService> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<string> CreateMoMoPaymentAsync(Order order, string returnUrl, string notifyUrl)
        {
            try
            {
                var momoSettings = _configuration.GetSection("MoMoPayment");
                var partnerCode = momoSettings["PartnerCode"] ?? throw new InvalidOperationException("MoMo PartnerCode not configured");
                var accessKey = momoSettings["AccessKey"] ?? throw new InvalidOperationException("MoMo AccessKey not configured");
                var secretKey = momoSettings["SecretKey"] ?? throw new InvalidOperationException("MoMo SecretKey not configured");
                var endpoint = momoSettings["Endpoint"] ?? throw new InvalidOperationException("MoMo Endpoint not configured");

                var requestId = Guid.NewGuid().ToString();
                var orderId = order.OrderNumber;
                var amount = (long)order.TotalAmount;
                var orderInfo = $"Thanh toan don hang {orderId}";
                var extraData = "";

                // Create raw signature
                var rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType=captureMoMoWallet";

                var signature = ComputeHmacSha256(rawHash, secretKey);

                var requestBody = new
                {
                    partnerCode = partnerCode,
                    partnerName = "Course Manager",
                    storeId = "CourseManagerStore",
                    requestId = requestId,
                    amount = amount,
                    orderId = orderId,
                    orderInfo = orderInfo,
                    redirectUrl = returnUrl,
                    ipnUrl = notifyUrl,
                    lang = "vi",
                    extraData = extraData,
                    requestType = "captureMoMoWallet",
                    signature = signature
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    if (responseData.TryGetProperty("payUrl", out var payUrl))
                    {
                        _logger.LogInformation("MoMo payment URL created successfully for order {OrderId}", orderId);
                        return payUrl.GetString() ?? string.Empty;
                    }
                }

                _logger.LogError("Failed to create MoMo payment for order {OrderId}. Response: {Response}", orderId, responseContent);
                throw new InvalidOperationException("Failed to create MoMo payment");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating MoMo payment for order {OrderId}", order.OrderNumber);
                throw;
            }
        }

        public async Task<bool> VerifyMoMoPaymentAsync(string orderId, string requestId, int resultCode)
        {
            try
            {
                // MoMo payment verification logic
                // In a real implementation, you would verify the payment with MoMo's API
                _logger.LogInformation("MoMo payment verification for order {OrderId} with result code {ResultCode}", orderId, resultCode);
                
                // For demo purposes, consider resultCode 0 as success
                return resultCode == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while verifying MoMo payment for order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> ProcessPaymentCallbackAsync(string orderId, string status, string? transactionId = null)
        {
            try
            {
                _logger.LogInformation("Processing payment callback for order {OrderId} with status {Status}", orderId, status);
                
                // In a real implementation, you would update the order status in the database
                // and handle the payment confirmation
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing payment callback for order {OrderId}", orderId);
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetPaymentMethodsAsync()
        {
            try
            {
                var paymentMethods = new Dictionary<string, object>
                {
                    ["momo"] = new
                    {
                        name = "MoMo",
                        description = "Thanh toán qua ví MoMo",
                        icon = "wallet",
                        enabled = true
                    },
                    ["banking"] = new
                    {
                        name = "Chuyển khoản ngân hàng",
                        description = "Chuyển khoản trực tiếp",
                        icon = "bank",
                        enabled = true
                    },
                    ["cod"] = new
                    {
                        name = "Thanh toán khi nhận khóa học",
                        description = "COD - Cash on Delivery",
                        icon = "credit-card",
                        enabled = true
                    }
                };

                return await Task.FromResult(paymentMethods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting payment methods");
                throw;
            }
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }
    }
}
