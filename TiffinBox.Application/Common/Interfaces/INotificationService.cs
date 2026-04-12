using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.Common.Interfaces
{
    public interface INotificationService
    {
        // Subscription Notifications
        Task SendSubscriptionCreatedAsync(Guid customerId, Guid subscriptionId);
        Task SendSubscriptionCancelledAsync(Guid customerId, Guid subscriptionId);
        Task SendSubscriptionPausedAsync(Guid customerId, Guid subscriptionId);
        Task SendSubscriptionResumedAsync(Guid customerId, Guid subscriptionId);
        Task SendSubscriptionExpiringAsync(Guid customerId, Guid subscriptionId, int daysLeft);

        // Order Notifications
        Task SendOrderPlacedAsync(Guid customerId, Guid orderId);
        Task SendOrderConfirmedAsync(Guid customerId, Guid orderId);
        Task SendOrderPreparingAsync(Guid customerId, Guid orderId);
        Task SendOrderOutForDeliveryAsync(Guid customerId, Guid orderId, string? deliveryAgentName = null);
        Task SendOrderDeliveredAsync(Guid customerId, Guid orderId);
        Task SendOrderCancelledAsync(Guid customerId, Guid orderId, string reason);

        // Payment Notifications
        Task SendPaymentSuccessAsync(Guid customerId, string paymentId, decimal amount);
        Task SendPaymentFailedAsync(Guid customerId, string paymentId, string errorMessage);
        Task SendPaymentRefundedAsync(Guid customerId, string paymentId, decimal amount);

        // Wallet Notifications
        Task SendWalletCreditedAsync(Guid customerId, decimal amount, string description);
        Task SendWalletDebitedAsync(Guid customerId, decimal amount, string description);
        Task SendWalletLowBalanceAsync(Guid customerId, decimal balance);

        // Vendor Related Notifications
        Task SendNewOrderToVendorAsync(Guid vendorId, Guid orderId);
        Task SendOrderStatusUpdateToVendorAsync(Guid vendorId, Guid orderId, string status);

        // Delivery Agent Notifications
        Task SendNewOrderToDeliveryAgentAsync(Guid agentId, Guid orderId);
        Task SendDeliveryAssignmentAsync(Guid agentId, Guid orderId);

        // General Notifications
        Task SendWelcomeEmailAsync(Guid userId, string email);
        Task SendEmailVerificationAsync(string email, string otp);
        Task SendPasswordResetAsync(string email, string token);
        Task SendSmsAsync(string phoneNumber, string message);
        Task SendPushNotificationAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null);
    }
}
