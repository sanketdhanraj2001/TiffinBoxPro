using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.Common.Interfaces
{
    public interface ISmsService
    {
        Task SendSmsAsync(string phoneNumber, string message);
        // NEW: OTP verification for registration
        Task<string> SendOtpVerificationAsync(string phoneNumber, string otp);
        Task<bool> VerifyOtpAsync(string sessionId, string otp);
        Task SendOtpAsync(string phoneNumber, string otp);
        Task SendDeliveryStatusAsync(string phoneNumber, string orderId, string status);
        Task SendOrderConfirmationAsync(string phoneNumber, string orderId, string vendorName, string deliveryDate);
        Task SendSubscriptionReminderAsync(string phoneNumber, string subscriptionId, int daysRemaining);
        Task SendPaymentConfirmationAsync(string phoneNumber, decimal amount, string transactionId);
        Task SendWalletUpdateAsync(string phoneNumber, decimal amount, string transactionType, decimal balance);
        Task SendPromotionalSmsAsync(string phoneNumber, string message);
        bool IsValidPhoneNumber(string phoneNumber);
        string FormatPhoneNumber(string phoneNumber, string countryCode = "+91");
    }
}
