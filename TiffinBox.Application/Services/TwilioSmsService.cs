using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Application.Common.Interfaces;
using TiffinBox.Application.Common.Settings;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace TiffinBox.Application.Services
{
    public class TwilioSmsService : ISmsService
    {
        private readonly SmsSettings _smsSettings;
        private readonly ILogger<TwilioSmsService> _logger;

        public TwilioSmsService(
            IOptions<SmsSettings> smsSettings,
            ILogger<TwilioSmsService> logger)
        {
            _smsSettings = smsSettings.Value;
            _logger = logger;
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                TwilioClient.Init(_smsSettings.AccountSid, _smsSettings.AuthToken);

                var formattedNumber = FormatPhoneNumber(phoneNumber);

                var smsMessage = await MessageResource.CreateAsync(
                    body: message,
                    from: new Twilio.Types.PhoneNumber(_smsSettings.FromPhoneNumber),
                    to: new Twilio.Types.PhoneNumber(formattedNumber)
                );

                _logger.LogInformation("SMS sent to {PhoneNumber}. SID: {Sid}", phoneNumber, smsMessage.Sid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        // Implement other methods similarly...
        public async Task SendOtpAsync(string phoneNumber, string otp)
        {
            await SendSmsAsync(phoneNumber, $"Your OTP is: {otp}");
        }

        public async Task SendDeliveryStatusAsync(string phoneNumber, string orderId, string status)
        {
            await SendSmsAsync(phoneNumber, $"Order #{orderId} is now {status}");
        }

        public async Task SendOrderConfirmationAsync(string phoneNumber, string orderId, string vendorName, string deliveryDate)
        {
            await SendSmsAsync(phoneNumber, $"Order #{orderId} confirmed from {vendorName} for {deliveryDate}");
        }

        public async Task SendSubscriptionReminderAsync(string phoneNumber, string subscriptionId, int daysRemaining)
        {
            await SendSmsAsync(phoneNumber, $"Subscription #{subscriptionId} expires in {daysRemaining} days");
        }

        public async Task SendPaymentConfirmationAsync(string phoneNumber, decimal amount, string transactionId)
        {
            await SendSmsAsync(phoneNumber, $"Payment of ₹{amount} received. ID: {transactionId}");
        }

        public async Task SendWalletUpdateAsync(string phoneNumber, decimal amount, string transactionType, decimal balance)
        {
            await SendSmsAsync(phoneNumber, $"₹{amount} {transactionType}ed. Balance: ₹{balance}");
        }

        public async Task SendPromotionalSmsAsync(string phoneNumber, string message)
        {
            await SendSmsAsync(phoneNumber, message);
        }

        public bool IsValidPhoneNumber(string phoneNumber)
        {
            return !string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.Length >= 10;
        }

        public string FormatPhoneNumber(string phoneNumber, string countryCode = "+91")
        {
            var digits = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"[^\d]", "");
            if (digits.Length == 10)
                return $"{countryCode}{digits}";
            if (digits.Length == 12 && digits.StartsWith("91"))
                return $"+{digits}";
            return phoneNumber.StartsWith("+") ? phoneNumber : $"{countryCode}{digits}";
        }
    }
}
