using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendVerificationEmailAsync(string email, string otp);
        Task SendPasswordResetEmailAsync(string email, string token);
        Task SendOrderConfirmationAsync(string email, string orderId, string vendorName, decimal totalAmount, DateTime deliveryDate);
        Task SendSubscriptionCreatedAsync(string email, string subscriptionId, string planName, DateTime startDate, DateTime endDate);
        Task SendPaymentReceiptAsync(string email, string paymentId, decimal amount, string transactionId);
        Task SendDeliveryStatusUpdateAsync(string email, string orderId, string status, string? trackingUrl = null);
        Task SendWelcomeEmailAsync(string email, string userName);
    }
}
