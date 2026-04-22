using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TiffinBox.Application.Common.Interfaces;
using TiffinBox.Application.Common.Settings;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace TiffinBox.Application.Services
{
    public class SmsService : ISmsService
    {
        private readonly SmsSettings _smsSettings;
        private readonly ILogger<SmsService> _logger;
        private readonly HttpClient _httpClient;

        public SmsService(IOptions<SmsSettings> smsSettings, ILogger<SmsService> logger)
        {
            _smsSettings = smsSettings.Value;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                var formattedNumber = FormatPhoneNumber(phoneNumber);

                if (!IsValidPhoneNumber(formattedNumber))
                {
                    _logger.LogWarning("Invalid phone number: {PhoneNumber}", phoneNumber);
                    throw new ArgumentException($"Invalid phone number: {phoneNumber}");
                }

                if (_smsSettings.Provider == "2Factor")
                {
                    await SendVia2Factor(formattedNumber);
                }
                else if (_smsSettings.Provider == "Mock")
                {
                    _logger.LogInformation("MOCK SMS - To: {PhoneNumber}, Message: {Message}", formattedNumber, message);
                }
                else
                {
                    _logger.LogWarning("Unknown SMS provider: {Provider}", _smsSettings.Provider);
                }

                _logger.LogInformation("SMS sent successfully to {PhoneNumber}", formattedNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        // ✅ 2Factor SMS OTP - uses approved template
        private async Task SendVia2Factor(string phoneNumber)
        {
            try
            {
                var formattedNumber = FormatPhoneNumberFor2Factor(phoneNumber);
                var templateName = "TiffinBox Pro";

                var url = $"https://2factor.in/API/V1/{_smsSettings.ApiKey}/SMS/{formattedNumber}/AUTOGEN/{templateName}";

                _logger.LogInformation("2Factor Request URL: {Url}", url.Replace(_smsSettings.ApiKey, "***HIDDEN***"));

                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("2Factor Response: {Response}", responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("2Factor Error - StatusCode: {StatusCode}, Response: {Response}", response.StatusCode, responseBody);
                    throw new Exception($"2Factor failed: {responseBody}");
                }

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;
                var status = root.GetProperty("Status").GetString();

                if (status != "Success")
                {
                    var details = root.GetProperty("Details").GetString();
                    _logger.LogError("2Factor API Error: {Details}", details);
                    throw new Exception($"2Factor error: {details}");
                }

                var sessionId = root.GetProperty("Details").GetString();
                _logger.LogInformation("2Factor - OTP sent successfully. SessionId: {SessionId}", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "2Factor - Failed to send SMS to {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        // ✅ Format phone number for 2Factor API (e.g., 917666163523)
        private string FormatPhoneNumberFor2Factor(string phoneNumber)
        {
            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

            if (digits.StartsWith("0"))
                digits = digits.Substring(1);

            if (!digits.StartsWith("91"))
                digits = "91" + digits;

            return digits;
        }

        // ✅ Verify OTP using session ID from 2Factor
        public async Task<bool> VerifyOtpAsync(string sessionId, string otp)
        {
            try
            {
                var url = $"https://2factor.in/API/V1/{_smsSettings.ApiKey}/SMS/VERIFY/{sessionId}/{otp}";

                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;
                var status = root.GetProperty("Status").GetString();

                return status == "Success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP");
                return false;
            }
        }

        public async Task SendOtpVerificationAsync(string phoneNumber, string otp)
        {
            await SendSmsAsync(phoneNumber, null);
        }

        public async Task SendOtpAsync(string phoneNumber, string otp)
        {
            await SendSmsAsync(phoneNumber, null);
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
            var message = $"TiffinBox Pro: Your subscription #{subscriptionId} has {daysRemaining} days remaining. Renew now!";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendPaymentConfirmationAsync(string phoneNumber, decimal amount, string transactionId)
        {
            var message = $"TiffinBox Pro: Payment of ₹{amount} received successfully. Transaction ID: {transactionId}.";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendWalletUpdateAsync(string phoneNumber, decimal amount, string transactionType, decimal balance)
        {
            var message = $"TiffinBox Pro: ₹{amount} has been {transactionType}ed from your wallet. Balance: ₹{balance}.";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendPromotionalSmsAsync(string phoneNumber, string message)
        {
            var promoMessage = $"TiffinBox Pro: {message}";
            await SendSmsAsync(phoneNumber, promoMessage);
        }

        public bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;
            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
            return digits.Length == 10 || (digits.Length == 12 && digits.StartsWith("91"));
        }

        public string FormatPhoneNumber(string phoneNumber, string countryCode = "+91")
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return phoneNumber;

            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

            if (digits.StartsWith("0"))
                digits = digits[1..];

            if (digits.Length == 10)
                return $"{countryCode}{digits}";

            if (digits.Length == 12 && digits.StartsWith("91"))
                return $"+{digits}";

            if (phoneNumber.StartsWith("+"))
                return phoneNumber;

            return $"{countryCode}{digits}";
        }
    }
}
