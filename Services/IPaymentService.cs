using CourseManager.API.Models;

namespace CourseManager.API.Services
{
    public interface IPaymentService
    {
        Task<string> CreateMoMoPaymentAsync(Order order, string returnUrl, string notifyUrl);
        Task<bool> VerifyMoMoPaymentAsync(string orderId, string requestId, int resultCode);
        Task<bool> ProcessPaymentCallbackAsync(string orderId, string status, string? transactionId = null);
        Task<Dictionary<string, object>> GetPaymentMethodsAsync();
    }
}
