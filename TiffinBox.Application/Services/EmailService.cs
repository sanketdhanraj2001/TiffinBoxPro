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
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Email Verification</title>
                    <style>
                        body {{
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            background-color: #f8f9fa;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 0 auto;
                            background-color: #ffffff;
                            border-radius: 12px;
                            overflow: hidden;
                            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                        }}
                        .header {{
                            background: linear-gradient(135deg, #f97316 0%, #ea580c 100%);
                            color: white;
                            padding: 30px 20px;
                            text-align: center;
                        }}
                        .header h1 {{
                            margin: 0;
                            font-size: 28px;
                            font-weight: bold;
                            letter-spacing: 1px;
                        }}
                        .header p {{
                            margin: 10px 0 0;
                            font-size: 14px;
                            opacity: 0.9;
                        }}
                        .content {{
                            padding: 40px 30px;
                            background-color: #ffffff;
                        }}
                        .content h2 {{
                            color: #333333;
                            font-size: 24px;
                            margin-top: 0;
                            margin-bottom: 20px;
                        }}
                        .content p {{
                            color: #666666;
                            font-size: 16px;
                            line-height: 1.5;
                            margin-bottom: 20px;
                        }}
                        .otp-box {{
                            background: linear-gradient(135deg, #fff7ed 0%, #ffedd5 100%);
                            border: 2px dashed #f97316;
                            border-radius: 12px;
                            padding: 20px;
                            text-align: center;
                            margin: 25px 0;
                        }}
                        .otp {{
                            font-size: 36px;
                            font-weight: bold;
                            color: #ea580c;
                            letter-spacing: 8px;
                            font-family: 'Courier New', monospace;
                        }}
                        .otp-label {{
                            font-size: 14px;
                            color: #f97316;
                            margin-top: 10px;
                            display: block;
                        }}
                        .info-box {{
                            background-color: #f8f9fa;
                            border-left: 4px solid #f97316;
                            padding: 15px 20px;
                            margin: 25px 0;
                            border-radius: 8px;
                        }}
                        .info-box p {{
                            margin: 0;
                            font-size: 14px;
                        }}
                        .button {{
                            display: inline-block;
                            background: linear-gradient(135deg, #f97316 0%, #ea580c 100%);
                            color: white;
                            text-decoration: none;
                            padding: 12px 30px;
                            border-radius: 25px;
                            font-weight: bold;
                            margin: 20px 0;
                            text-align: center;
                        }}
                        .footer {{
                            background-color: #f8f9fa;
                            padding: 20px;
                            text-align: center;
                            border-top: 1px solid #e9ecef;
                        }}
                        .footer p {{
                            margin: 5px 0;
                            font-size: 12px;
                            color: #999999;
                        }}
                        .footer a {{
                            color: #f97316;
                            text-decoration: none;
                        }}
                        .warning {{
                            color: #dc2626;
                            font-size: 12px;
                            margin-top: 15px;
                        }}
                    </style>
                </head>
                <body style='background-color: #f8f9fa; padding: 20px;'>
                    <div class='container'>
                        <div class='header'>
                            <h1>TiffinBox Pro</h1>
                            <p>Fresh Home-Cooked Meals Delivered</p>
                        </div>
                        
                        <div class='content'>
                            <h2>Verify Your Email Address</h2>
                            <p>Thank you for choosing <strong>TiffinBox Pro</strong>! Please verify your email address to complete your registration and start enjoying fresh, home-cooked meals.</p>
                            
                            <div class='otp-box'>
                                <div class='otp'>{otp}</div>
                                <span class='otp-label'>Your One-Time Password (OTP)</span>
                            </div>
                            
                            <div class='info-box'>
                                <p>✅ This OTP is valid for <strong>10 minutes</strong></p>
                                <p>🔒 For security reasons, do not share this OTP with anyone</p>
                                <p>📧 If you didn't request this verification, please ignore this email</p>
                            </div>
                            
                            <p style='text-align: center; margin-top: 30px;'>
                                <span style='color: #999;'>Need help? Contact our support team at</span>
                                <br>
                                <a href='mailto:support@tiffinbox.com' style='color: #f97316;'>support@tiffinbox.com</a>
                            </p>
                        </div>
                        
                        <div class='footer'>
                            <p>&copy; 2024 TiffinBox Pro. All rights reserved.</p>
                            <p>
                                <a href='#'>Privacy Policy</a> | 
                                <a href='#'>Terms of Service</a> | 
                                <a href='#'>Contact Us</a>
                            </p>
                            <p>Made with ❤️ for home-cooked meals</p>
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
