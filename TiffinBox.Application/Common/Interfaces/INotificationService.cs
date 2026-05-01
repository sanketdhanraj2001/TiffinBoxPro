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
        Task SendSubscriptionCreatedAsync(int customerId, int subscriptionId);
        Task SendSubscriptionCancelledAsync(int customerId, int subscriptionId);
        Task SendSubscriptionPausedAsync(int customerId, int subscriptionId);
        Task SendSubscriptionResumedAsync(int customerId, int subscriptionId);
        Task SendSubscriptionExpiringAsync(int customerId, int subscriptionId, int daysLeft);

        // Order Notifications
        Task SendOrderPlacedAsync(int customerId, int orderId);
        Task SendOrderConfirmedAsync(int customerId, int orderId);
        Task SendOrderPreparingAsync(int customerId, int orderId);
        Task SendOrderOutForDeliveryAsync(int customerId, int orderId, string? deliveryAgentName = null);
        Task SendOrderDeliveredAsync(int customerId, int orderId);
        Task SendOrderCancelledAsync(int customerId, int orderId, string reason);

        // Payment Notifications
        Task SendPaymentSuccessAsync(int customerId, string paymentId, decimal amount);
        Task SendPaymentFailedAsync(int customerId, string paymentId, string errorMessage);
        Task SendPaymentRefundedAsync(int customerId, string paymentId, decimal amount);

        // Wallet Notifications
        Task SendWalletCreditedAsync(int customerId, decimal amount, string description);
        Task SendWalletDebitedAsync(int customerId, decimal amount, string description);
        Task SendWalletLowBalanceAsync(int customerId, decimal balance);

        // Vendor Related Notifications
        Task SendNewOrderToVendorAsync(int vendorId, int orderId);
        Task SendOrderStatusUpdateToVendorAsync(int vendorId, int orderId, string status);

        // Delivery Agent Notifications
        Task SendNewOrderToDeliveryAgentAsync(int agentId, int orderId);
        Task SendDeliveryAssignmentAsync(int agentId, int orderId);

        // General Notifications
        Task SendWelcomeEmailAsync(int userId, string email);
        Task SendEmailVerificationAsync(string email, string otp);
        Task SendPasswordResetAsync(string email, string token);
        Task SendSmsAsync(string phoneNumber, string message);
        Task SendPushNotificationAsync(int userId, string title, string body, Dictionary<string, string>? data = null);
    }
}
