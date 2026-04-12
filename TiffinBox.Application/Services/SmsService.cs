using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TiffinBox.Application.Common.Interfaces;
using TiffinBox.Application.Common.Settings;

namespace TiffinBox.Application.Services
{
    public class SmsService : ISmsService
    {
        private readonly SmsSettings _smsSettings;
        private readonly ILogger<SmsService> _logger;

        public SmsService(
            IOptions<SmsSettings> smsSettings,
            ILogger<SmsService> logger)
        {
            _smsSettings = smsSettings.Value;
            _logger = logger;
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                var formattedNumber = FormatPhoneNumber(phoneNumber);

                if (!IsValidPhoneNumber(formattedNumber))
                {
                    _logger.LogWarning("Invalid phone number: {PhoneNumber}", phoneNumber);
                    return;
                }

                _logger.LogInformation("Sending SMS to {PhoneNumber}: {Message}", formattedNumber, message);

                // In production, integrate with actual SMS provider (Twilio, AWS SNS, etc.)
                // Example with Twilio:
                /*
                TwilioClient.Init(_smsSettings.AccountSid, _smsSettings.AuthToken);
                var message = await MessageResource.CreateAsync(
                    body: message,
                    from: new PhoneNumber(_smsSettings.FromPhoneNumber),
                    to: new PhoneNumber(formattedNumber)
                );
                */

                // For now, just log (production ready code would call actual API)
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        public async Task SendOtpAsync(string phoneNumber, string otp)
        {
            var message = $"Your TiffinBox Pro OTP is: {otp}. This OTP is valid for 10 minutes. Do not share it with anyone.";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendDeliveryStatusAsync(string phoneNumber, string orderId, string status)
        {
            var message = $"TiffinBox Pro: Your order #{orderId} is now {status}. Track your delivery in the app.";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendOrderConfirmationAsync(string phoneNumber, string orderId, string vendorName, string deliveryDate)
        {
            var message = $"TiffinBox Pro: Your order #{orderId} from {vendorName} has been confirmed. Delivery on {deliveryDate}. Thank you!";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendSubscriptionReminderAsync(string phoneNumber, string subscriptionId, int daysRemaining)
        {
            var message = $"TiffinBox Pro: Your subscription #{subscriptionId} has {daysRemaining} days remaining. Renew now to continue uninterrupted service.";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendPaymentConfirmationAsync(string phoneNumber, decimal amount, string transactionId)
        {
            var message = $"TiffinBox Pro: Payment of ₹{amount} received successfully. Transaction ID: {transactionId}. Thank you for your payment.";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendWalletUpdateAsync(string phoneNumber, decimal amount, string transactionType, decimal balance)
        {
            var message = $"TiffinBox Pro: ₹{amount} has been {transactionType}ed from your wallet. Current balance: ₹{balance}.";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendPromotionalSmsAsync(string phoneNumber, string message)
        {
            // Only send if user has opted in for promotional messages
            // Check user's marketing preferences before sending

            var promotionalMessage = $"TiffinBox Pro: {message}";
            await SendSmsAsync(phoneNumber, promotionalMessage);
        }

        public bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Remove any whitespace
            phoneNumber = phoneNumber.Trim();

            // Basic international phone number validation
            // Allows: +91XXXXXXXXXX, 91XXXXXXXXXX, 0XXXXXXXXXX
            var pattern = @"^(\+?\d{1,3}[- ]?)?\d{10}$";
            return Regex.IsMatch(phoneNumber, pattern);
        }

        public string FormatPhoneNumber(string phoneNumber, string countryCode = "+91")
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return phoneNumber;

            // Remove any non-digit characters
            var digits = Regex.Replace(phoneNumber, @"[^\d]", "");

            // If number starts with 0, remove it
            if (digits.StartsWith("0"))
                digits = digits[1..];

            // If number is 10 digits, add country code
            if (digits.Length == 10)
                return $"{countryCode}{digits}";

            // If number starts with country code (e.g., 91 for India)
            if (digits.Length == 12 && digits.StartsWith("91"))
                return $"+{digits}";

            // If number already has + prefix
            if (phoneNumber.StartsWith("+"))
                return phoneNumber;

            // Default: add country code
            return $"{countryCode}{digits}";
        }
    }
}
