using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Application.Common.Interfaces;


namespace TiffinBox.Application.Services
{

    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly IEmailService? _emailService;
        private readonly ISmsService? _smsService;

        public NotificationService(
            ILogger<NotificationService> logger,
            IEmailService? emailService = null,
            ISmsService? smsService = null)
        {
            _logger = logger;
            _emailService = emailService;
            _smsService = smsService;
        }

        // ==================== Subscription Notifications ====================

        public async Task SendSubscriptionCreatedAsync(Guid customerId, Guid subscriptionId)
        {
            _logger.LogInformation("📧 Subscription created notification sent to customer {CustomerId} for subscription {SubscriptionId}",
                customerId, subscriptionId);

            // In production, save to database and send real notifications
            await Task.CompletedTask;
        }

        public async Task SendSubscriptionCancelledAsync(Guid customerId, Guid subscriptionId)
        {
            _logger.LogInformation("📧 Subscription cancelled notification sent to customer {CustomerId} for subscription {SubscriptionId}",
                customerId, subscriptionId);
            await Task.CompletedTask;
        }

        public async Task SendSubscriptionPausedAsync(Guid customerId, Guid subscriptionId)
        {
            _logger.LogInformation("📧 Subscription paused notification sent to customer {CustomerId} for subscription {SubscriptionId}",
                customerId, subscriptionId);
            await Task.CompletedTask;
        }

        public async Task SendSubscriptionResumedAsync(Guid customerId, Guid subscriptionId)
        {
            _logger.LogInformation("📧 Subscription resumed notification sent to customer {CustomerId} for subscription {SubscriptionId}",
                customerId, subscriptionId);
            await Task.CompletedTask;
        }

        public async Task SendSubscriptionExpiringAsync(Guid customerId, Guid subscriptionId, int daysLeft)
        {
            _logger.LogInformation("📧 Subscription expiring notification sent to customer {CustomerId} for subscription {SubscriptionId}. Days left: {DaysLeft}",
                customerId, subscriptionId, daysLeft);
            await Task.CompletedTask;
        }

        // ==================== Order Notifications ====================

        public async Task SendOrderPlacedAsync(Guid customerId, Guid orderId)
        {
            _logger.LogInformation("📧 Order placed notification sent to customer {CustomerId} for order {OrderId}",
                customerId, orderId);
            await Task.CompletedTask;
        }

        public async Task SendOrderConfirmedAsync(Guid customerId, Guid orderId)
        {
            _logger.LogInformation("📧 Order confirmed notification sent to customer {CustomerId} for order {OrderId}",
                customerId, orderId);
            await Task.CompletedTask;
        }

        public async Task SendOrderPreparingAsync(Guid customerId, Guid orderId)
        {
            _logger.LogInformation("📧 Order preparing notification sent to customer {CustomerId} for order {OrderId}",
                customerId, orderId);
            await Task.CompletedTask;
        }

        public async Task SendOrderOutForDeliveryAsync(Guid customerId, Guid orderId, string? deliveryAgentName = null)
        {
            _logger.LogInformation("📧 Order out for delivery notification sent to customer {CustomerId} for order {OrderId}. Delivery Agent: {AgentName}",
                customerId, orderId, deliveryAgentName ?? "Not assigned");
            await Task.CompletedTask;
        }

        public async Task SendOrderDeliveredAsync(Guid customerId, Guid orderId)
        {
            _logger.LogInformation("📧 Order delivered notification sent to customer {CustomerId} for order {OrderId}",
                customerId, orderId);
            await Task.CompletedTask;
        }

        public async Task SendOrderCancelledAsync(Guid customerId, Guid orderId, string reason)
        {
            _logger.LogInformation("📧 Order cancelled notification sent to customer {CustomerId} for order {OrderId}. Reason: {Reason}",
                customerId, orderId, reason);
            await Task.CompletedTask;
        }

        // ==================== Payment Notifications ====================

        public async Task SendPaymentSuccessAsync(Guid customerId, string paymentId, decimal amount)
        {
            _logger.LogInformation("💰 Payment success notification sent to customer {CustomerId} for payment {PaymentId}. Amount: {Amount}",
                customerId, paymentId, amount);
            await Task.CompletedTask;
        }

        public async Task SendPaymentFailedAsync(Guid customerId, string paymentId, string errorMessage)
        {
            _logger.LogError("❌ Payment failed notification sent to customer {CustomerId} for payment {PaymentId}. Error: {ErrorMessage}",
                customerId, paymentId, errorMessage);
            await Task.CompletedTask;
        }

        public async Task SendPaymentRefundedAsync(Guid customerId, string paymentId, decimal amount)
        {
            _logger.LogInformation("💰 Payment refunded notification sent to customer {CustomerId} for payment {PaymentId}. Amount: {Amount}",
                customerId, paymentId, amount);
            await Task.CompletedTask;
        }

        // ==================== Wallet Notifications ====================

        public async Task SendWalletCreditedAsync(Guid customerId, decimal amount, string description)
        {
            _logger.LogInformation("💰 Wallet credited notification sent to customer {CustomerId}. Amount: {Amount}, Description: {Description}",
                customerId, amount, description);
            await Task.CompletedTask;
        }

        public async Task SendWalletDebitedAsync(Guid customerId, decimal amount, string description)
        {
            _logger.LogInformation("💰 Wallet debited notification sent to customer {CustomerId}. Amount: {Amount}, Description: {Description}",
                customerId, amount, description);
            await Task.CompletedTask;
        }

        public async Task SendWalletLowBalanceAsync(Guid customerId, decimal balance)
        {
            _logger.LogWarning("⚠️ Low wallet balance notification sent to customer {CustomerId}. Current Balance: {Balance}",
                customerId, balance);
            await Task.CompletedTask;
        }

        // ==================== Vendor Related Notifications ====================

        public async Task SendNewOrderToVendorAsync(Guid vendorId, Guid orderId)
        {
            _logger.LogInformation("📦 New order notification sent to vendor {VendorId} for order {OrderId}",
                vendorId, orderId);
            await Task.CompletedTask;
        }

        public async Task SendOrderStatusUpdateToVendorAsync(Guid vendorId, Guid orderId, string status)
        {
            _logger.LogInformation("📦 Order status update sent to vendor {VendorId} for order {OrderId}. Status: {Status}",
                vendorId, orderId, status);
            await Task.CompletedTask;
        }

        // ==================== Delivery Agent Notifications ====================

        public async Task SendNewOrderToDeliveryAgentAsync(Guid agentId, Guid orderId)
        {
            _logger.LogInformation("🛵 New delivery assignment notification sent to agent {AgentId} for order {OrderId}",
                agentId, orderId);
            await Task.CompletedTask;
        }

        public async Task SendDeliveryAssignmentAsync(Guid agentId, Guid orderId)
        {
            _logger.LogInformation("🛵 Delivery assignment notification sent to agent {AgentId} for order {OrderId}",
                agentId, orderId);
            await Task.CompletedTask;
        }

        // ==================== General Notifications ====================

        public async Task SendWelcomeEmailAsync(Guid userId, string email)
        {
            _logger.LogInformation("📧 Welcome email sent to {Email} (User: {UserId})", email, userId);

            if (_emailService != null)
            {
                await _emailService.SendEmailAsync(email, "Welcome to TiffinBox Pro!",
                    $"<h1>Welcome {email}!</h1><p>Thank you for joining TiffinBox Pro.</p>");
            }
        }

        public async Task SendEmailVerificationAsync(string email, string otp)
        {
            _logger.LogInformation("📧 Email verification OTP sent to {Email}. OTP: {Otp}", email, otp);

            if (_emailService != null)
            {
                await _emailService.SendEmailAsync(email, "Verify Your Email",
                    $"<h1>Email Verification</h1><p>Your OTP is: <strong>{otp}</strong></p><p>This OTP will expire in 10 minutes.</p>");
            }
        }

        public async Task SendPasswordResetAsync(string email, string token)
        {
            _logger.LogInformation("📧 Password reset link sent to {Email}. Token: {Token}", email, token);

            if (_emailService != null)
            {
                var resetLink = $"https://tiffinbox.com/reset-password?token={token}&email={email}";
                await _emailService.SendEmailAsync(email, "Reset Your Password",
                    $"<h1>Password Reset</h1><p>Click <a href='{resetLink}'>here</a> to reset your password.</p><p>This link will expire in 1 hour.</p>");
            }
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            _logger.LogInformation("📱 SMS sent to {PhoneNumber}. Message: {Message}", phoneNumber, message);

            if (_smsService != null)
            {
                await _smsService.SendSmsAsync(phoneNumber, message);
            }
        }

        public async Task SendPushNotificationAsync(Guid userId, string title, string body, Dictionary<string, string>? data = null)
        {
            _logger.LogInformation("🔔 Push notification sent to user {UserId}. Title: {Title}, Body: {Body}",
                userId, title, body);

            // In production, implement push notification via Firebase or similar
            await Task.CompletedTask;
        }
    }
}
