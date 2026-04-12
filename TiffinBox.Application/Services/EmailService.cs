using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Application.Common.Interfaces;
using TiffinBox.Application.Common.Settings;

namespace TiffinBox.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                _logger.LogInformation("Sending email to {To} with subject {Subject}", to, subject);

                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                    EnableSsl = _emailSettings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }

        public async Task SendVerificationEmailAsync(string email, string otp)
        {
            var subject = "Verify Your Email - TiffinBox Pro";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .otp {{ font-size: 32px; font-weight: bold; color: #4CAF50; text-align: center; padding: 20px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>TiffinBox Pro</h1>
                        </div>
                        <div class='content'>
                            <h2>Email Verification</h2>
                            <p>Thank you for registering with TiffinBox Pro!</p>
                            <p>Please use the following OTP to verify your email address:</p>
                            <div class='otp'>{otp}</div>
                            <p>This OTP is valid for 10 minutes.</p>
                            <p>If you didn't request this, please ignore this email.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 TiffinBox Pro. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string email, string token)
        {
            var resetLink = $"https://tiffinbox.com/reset-password?token={token}&email={email}";
            var subject = "Reset Your Password - TiffinBox Pro";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .button {{ background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; display: inline-block; border-radius: 4px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>TiffinBox Pro</h1>
                        </div>
                        <div class='content'>
                            <h2>Password Reset Request</h2>
                            <p>We received a request to reset your password.</p>
                            <p>Click the button below to reset your password:</p>
                            <p style='text-align: center;'>
                                <a href='{resetLink}' class='button'>Reset Password</a>
                            </p>
                            <p>Or copy and paste this link into your browser:</p>
                            <p>{resetLink}</p>
                            <p>This link is valid for 1 hour.</p>
                            <p>If you didn't request this, please ignore this email.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 TiffinBox Pro. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendOrderConfirmationAsync(string email, string orderId, string vendorName, decimal totalAmount, DateTime deliveryDate)
        {
            var subject = $"Order Confirmation #{orderId} - TiffinBox Pro";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .order-details {{ background-color: #f9f9f9; padding: 15px; border-radius: 4px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>TiffinBox Pro</h1>
                        </div>
                        <div class='content'>
                            <h2>Order Confirmation</h2>
                            <p>Thank you for your order!</p>
                            <div class='order-details'>
                                <p><strong>Order ID:</strong> {orderId}</p>
                                <p><strong>Vendor:</strong> {vendorName}</p>
                                <p><strong>Total Amount:</strong> ₹{totalAmount:F2}</p>
                                <p><strong>Delivery Date:</strong> {deliveryDate:dd MMM yyyy}</p>
                            </div>
                            <p>You can track your order in the app.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 TiffinBox Pro. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendSubscriptionCreatedAsync(string email, string subscriptionId, string planName, DateTime startDate, DateTime endDate)
        {
            var subject = $"Subscription Created - TiffinBox Pro";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .subscription-details {{ background-color: #f9f9f9; padding: 15px; border-radius: 4px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>TiffinBox Pro</h1>
                        </div>
                        <div class='content'>
                            <h2>Subscription Created Successfully!</h2>
                            <div class='subscription-details'>
                                <p><strong>Subscription ID:</strong> {subscriptionId}</p>
                                <p><strong>Plan:</strong> {planName}</p>
                                <p><strong>Start Date:</strong> {startDate:dd MMM yyyy}</p>
                                <p><strong>End Date:</strong> {endDate:dd MMM yyyy}</p>
                            </div>
                            <p>Your meals will be delivered according to your selected schedule.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 TiffinBox Pro. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPaymentReceiptAsync(string email, string paymentId, decimal amount, string transactionId)
        {
            var subject = $"Payment Receipt - TiffinBox Pro";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .payment-details {{ background-color: #f9f9f9; padding: 15px; border-radius: 4px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>TiffinBox Pro</h1>
                        </div>
                        <div class='content'>
                            <h2>Payment Receipt</h2>
                            <div class='payment-details'>
                                <p><strong>Payment ID:</strong> {paymentId}</p>
                                <p><strong>Transaction ID:</strong> {transactionId}</p>
                                <p><strong>Amount:</strong> ₹{amount:F2}</p>
                                <p><strong>Status:</strong> Completed</p>
                            </div>
                            <p>Thank you for your payment!</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 TiffinBox Pro. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendDeliveryStatusUpdateAsync(string email, string orderId, string status, string? trackingUrl = null)
        {
            var subject = $"Delivery Update for Order #{orderId} - TiffinBox Pro";
            var trackingHtml = string.IsNullOrEmpty(trackingUrl)
                ? ""
                : $"<p><strong>Track your delivery:</strong> <a href='{trackingUrl}'>Click here</a></p>";

            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>TiffinBox Pro</h1>
                        </div>
                        <div class='content'>
                            <h2>Delivery Status Update</h2>
                            <p>Your order <strong>#{orderId}</strong> is now: <strong>{status}</strong></p>
                            {trackingHtml}
                            <p>Thank you for choosing TiffinBox Pro!</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 TiffinBox Pro. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string email, string userName)
        {
            var subject = "Welcome to TiffinBox Pro!";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>TiffinBox Pro</h1>
                        </div>
                        <div class='content'>
                            <h2>Welcome, {userName}!</h2>
                            <p>We're excited to have you on board!</p>
                            <p>With TiffinBox Pro, you can:</p>
                            <ul>
                                <li>Order delicious home-cooked meals</li>
                                <li>Subscribe to daily, weekly, or monthly plans</li>
                                <li>Track your deliveries in real-time</li>
                                <li>Manage your wallet and payments</li>
                            </ul>
                            <p>Get started by browsing our vendors and placing your first order!</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2024 TiffinBox Pro. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }
    }
}
