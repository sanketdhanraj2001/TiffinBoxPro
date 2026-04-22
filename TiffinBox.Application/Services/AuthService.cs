using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Application.Common.Interfaces;
using TiffinBox.Application.Common.Models;
using TiffinBox.Application.Common.Settings;
using TiffinBox.Application.DTOs.Auth;
using TiffinBox.Application.DTOs.Common;
using TiffinBox.Domain.Entities;
using TiffinBox.Domain.Enums;
using TiffinBox.Domain.Interfaces;

namespace TiffinBox.Application.Services
{
    public class AuthService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ICacheService _cacheService;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ISmsService smsService,
            ICacheService cacheService,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _smsService = smsService;
            _cacheService = cacheService;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByEmailAsync(request.EmailOrUserName);

                if (user == null)
                    return ApiResponse<LoginResponseDto>.Fail("Invalid email or password");

                if (user.IsLockedOut())
                    return ApiResponse<LoginResponseDto>.Fail("Account is locked. Please try again later.");

                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    user.RecordLoginFailure();
                    await _unitOfWork.CompleteAsync();
                    return ApiResponse<LoginResponseDto>.Fail("Invalid email or password");
                }

                if (!user.IsEmailVerified)
                    return ApiResponse<LoginResponseDto>.Fail("Please verify your email before logging in");

                if (!user.IsActive)
                    return ApiResponse<LoginResponseDto>.Fail("Your account has been deactivated");

                user.RecordLoginSuccess();

                var (accessToken, expiresAt) = GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken();

                user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays));
                await _unitOfWork.CompleteAsync();

                var userProfile = MapToUserProfileDto(user);

                var response = new LoginResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt,
                    User = userProfile,
                    TokenType = "Bearer",
                    ExpiresIn = (int)(expiresAt - DateTime.UtcNow).TotalSeconds
                };

                _logger.LogInformation("User {Email} logged in successfully", user.Email);
                return ApiResponse<LoginResponseDto>.Ok(response, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for {Email}", request.EmailOrUserName);
                return ApiResponse<LoginResponseDto>.Fail("An error occurred during login");
            }
        }

        public async Task<ApiResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            try
            {
                var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
                if (existingUser != null)
                    return ApiResponse<RegisterResponseDto>.Fail("Email already registered");

                existingUser = await _unitOfWork.Users.GetByPhoneAsync(request.PhoneNumber);
                if (existingUser != null)
                    return ApiResponse<RegisterResponseDto>.Fail("Phone number already registered");

                await _unitOfWork.BeginTransactionAsync();

                var user = User.Create(request.Email, request.PhoneNumber, request.FirstName, request.LastName, request.Role);
                user.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword(request.Password));

                if (request.Address != null)
                {
                    user.UpdateProfile(request.FirstName, request.LastName, request.Address.ToDomain(), null);
                }

                await _unitOfWork.Users.AddAsync(user);

                if (request.Role == UserRole.VendorAdmin)
                {
                    var vendor = Vendor.Create(user, $"{request.FirstName}'s Kitchen", user.Address!);
                    await _unitOfWork.Vendors.AddAsync(vendor);
                }

                await _unitOfWork.CompleteAsync();
                await _unitOfWork.CommitTransactionAsync();

                var otp = GenerateOtp();
                await _cacheService.SetAsync($"otp:{user.Email}", otp, TimeSpan.FromMinutes(10));
                await _emailService.SendVerificationEmailAsync(user.Email, otp);

                _logger.LogInformation("User {Email} registered successfully", user.Email);

                return ApiResponse<RegisterResponseDto>.Ok(
                    new RegisterResponseDto(user.Id, user.Email, "Registration successful. Please verify your email with OTP."));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Registration error for {Email}", request.Email);
                return ApiResponse<RegisterResponseDto>.Fail("An error occurred during registration");
            }
        }

        public async Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var user = await _unitOfWork.Users.GetByRefreshTokenAsync(request.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return ApiResponse<TokenResponseDto>.Fail("Invalid or expired refresh token");

            var (newAccessToken, expiresAt) = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays));
            await _unitOfWork.CompleteAsync();

            return ApiResponse<TokenResponseDto>.Ok(new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = expiresAt
            });
        }

        public async Task<ApiResponse<bool>> LogoutAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user != null)
            {
                user.RevokeRefreshToken();
                await _unitOfWork.CompleteAsync();
            }

            return ApiResponse<bool>.Ok(true, "Logged out successfully");
        }

        public async Task<ApiResponse<UserProfileDto>> GetCurrentUserAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdWithDetailsAsync(userId);

            if (user == null)
                return ApiResponse<UserProfileDto>.Fail("User not found");

            return ApiResponse<UserProfileDto>.Ok(MapToUserProfileDto(user));
        }

        public async Task<ApiResponse<bool>> VerifyOtpAsync(VerifyOtpRequest request)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(request.Email))
                    return ApiResponse<bool>.Fail("Please enter a valid email address");

                if (string.IsNullOrWhiteSpace(request.Otp) || request.Otp.Length != 6)
                    return ApiResponse<bool>.Fail("Please enter a valid 6-digit OTP");

                // Get cached OTP - FIXED: Use email_otp: instead of otp:
                var cachedOtp = await _cacheService.GetAsync<string>($"email_otp:{request.Email}");

                if (cachedOtp == null)
                {
                    _logger.LogWarning("Email OTP expired for {Email}", request.Email);
                    return ApiResponse<bool>.Fail("OTP has expired. Please request a new one.");
                }

                if (cachedOtp != request.Otp)
                {
                    _logger.LogWarning("Invalid email OTP attempt for {Email}. Expected: {Expected}, Got: {Actual}",
                        request.Email, cachedOtp, request.Otp);
                    return ApiResponse<bool>.Fail("Invalid OTP. Please enter the correct 6-digit OTP.");
                }

                // Find user by email
                var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

                if (user == null)
                {
                    // User doesn't exist - store verified email for registration
                    await _cacheService.SetAsync($"verified_email:{request.Email}", true, TimeSpan.FromMinutes(30));
                    await _cacheService.RemoveAsync($"email_otp:{request.Email}");

                    _logger.LogInformation("Email {Email} verified for new registration", request.Email);

                    return ApiResponse<bool>.Ok(true, "Email verified successfully! You can now complete your registration.");
                }

                // User exists - mark email as verified
                user.VerifyEmail();  // This sets IsEmailVerified = true
                await _unitOfWork.CompleteAsync();

                // Remove OTP from cache after successful verification
                await _cacheService.RemoveAsync($"email_otp:{request.Email}");

                _logger.LogInformation("Email {Email} verified successfully for user {UserId}", request.Email, user.Id);

                return ApiResponse<bool>.Ok(true, "Email verified successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email OTP for {Email}", request.Email);
                return ApiResponse<bool>.Fail("Failed to verify OTP. Please try again.");
            }
        }

        //public async Task<ApiResponse<bool>> VerifyOtpAsync(VerifyOtpRequest request)
        //{
        //    if (string.IsNullOrWhiteSpace(request.Email))
        //        return ApiResponse<bool>.Fail("Email is required");

        //    if (string.IsNullOrWhiteSpace(request.Otp) || request.Otp.Length != 6)
        //        return ApiResponse<bool>.Fail("Valid OTP is required");

        //    var cachedOtp = await _cacheService.GetAsync<string>($"otp:{request.Email}");

        //    if (cachedOtp == null)
        //        return ApiResponse<bool>.Fail("OTP has expired. Please request a new one.");

        //    if (cachedOtp != request.Otp)
        //        return ApiResponse<bool>.Fail("Invalid OTP. Please try again.");

        //    var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
        //    if (user == null)
        //        return ApiResponse<bool>.Fail("User not found");

        //    user.VerifyEmail();
        //    await _unitOfWork.CompleteAsync();
        //    await _cacheService.RemoveAsync($"otp:{request.Email}");

        //    return ApiResponse<bool>.Ok(true, "Email verified successfully");
        //}

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (user == null)
                return ApiResponse<bool>.Ok(true, "If the email exists, a reset link will be sent");

            var resetToken = GenerateResetToken();
            await _cacheService.SetAsync($"reset:{user.Email}", resetToken, TimeSpan.FromHours(1));
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

            return ApiResponse<bool>.Ok(true, "Password reset link sent to your email");
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var cachedToken = await _cacheService.GetAsync<string>($"reset:{request.Email}");

            if (cachedToken == null || cachedToken != request.Token)
                return ApiResponse<bool>.Fail("Invalid or expired reset token");

            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (user == null)
                return ApiResponse<bool>.Fail("User not found");

            user.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
            await _unitOfWork.CompleteAsync();
            await _cacheService.RemoveAsync($"reset:{request.Email}");

            return ApiResponse<bool>.Ok(true, "Password reset successfully");
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return ApiResponse<bool>.Fail("User not found");

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                return ApiResponse<bool>.Fail("Current password is incorrect");

            if (oldPassword == newPassword)
                return ApiResponse<bool>.Fail("New password cannot be the same as old password");

            user.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword(newPassword));
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.Ok(true, "Password changed successfully");
        }

        public async Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdWithDetailsAsync(userId);
            if (user == null)
                return ApiResponse<UserProfileDto>.Fail("User not found");

            var firstName = request.FirstName ?? user.FirstName;
            var lastName = request.LastName ?? user.LastName;
            var address = request.Address?.ToDomain() ?? user.Address;

            user.UpdateProfile(firstName, lastName, address, request.ProfilePictureUrl);

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && request.PhoneNumber != user.PhoneNumber)
            {
                var existingUser = await _unitOfWork.Users.GetByPhoneAsync(request.PhoneNumber);
                if (existingUser != null && existingUser.Id != userId)
                    return ApiResponse<UserProfileDto>.Fail("Phone number already in use");
            }

            await _unitOfWork.CompleteAsync();

            return ApiResponse<UserProfileDto>.Ok(MapToUserProfileDto(user), "Profile updated successfully");
        }

        public async Task<ApiResponse<bool>> ResendVerificationOtpAsync(string email)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null)
                return ApiResponse<bool>.Ok(true, "If the email exists, an OTP will be sent");

            if (user.IsEmailVerified)
                return ApiResponse<bool>.Fail("Email is already verified");

            var otp = GenerateOtp();
            await _cacheService.SetAsync($"otp:{user.Email}", otp, TimeSpan.FromMinutes(10));
            await _emailService.SendVerificationEmailAsync(user.Email, otp);

            return ApiResponse<bool>.Ok(true, "Verification OTP sent to your email");
        }

        // ==================== Private Helper Methods ====================

        private (string Token, DateTime ExpiresAt) GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            if (user.Vendor != null)
                claims.Add(new Claim("vendorId", user.Vendor.Id.ToString()));

            if (user.DeliveryAgent != null)
                claims.Add(new Claim("agentId", user.DeliveryAgent.Id.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private string GenerateResetToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        private UserProfileDto MapToUserProfileDto(User user)
        {
            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString(),
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = user.IsPhoneVerified,
                ProfilePictureUrl = user.ProfilePictureUrl,
                VendorId = user.Vendor?.Id,
                DeliveryAgentId = user.DeliveryAgent?.Id,
                WalletBalance = user.Wallet?.Balance.Amount ?? 0,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }


        // ==================== Mobile OTP Methods ====================

        /// Send OTP to mobile number for verification
        /// <summary>
        /// Helper method to extract 10-digit phone number from various formats
        /// </summary>
        private string Extract10DigitPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            // Extract only digits
            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // If we have 10 digits, return as is
            if (digits.Length == 10)
                return digits;

            // If we have 12 digits starting with 91 (India country code), take last 10
            if (digits.Length == 12 && digits.StartsWith("91"))
                return digits.Substring(2);

            // If we have more than 10 digits, take last 10
            if (digits.Length > 10)
                return digits.Substring(digits.Length - 10);

            // Return as is (will fail validation if not 10 digits)
            return digits;
        }

        /// <summary>
        /// Format phone number for SMS (adds +91 country code)
        /// </summary>
        private string FormatPhoneNumberForSms(string phoneNumber)
        {
            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

            if (digits.Length == 10)
                return $"+91{digits}";

            if (digits.Length == 12 && digits.StartsWith("91"))
                return $"+{digits}";

            if (phoneNumber.StartsWith("+"))
                return phoneNumber;

            return $"+91{digits}";
        }

        /// Send OTP to mobile number for verification
        public async Task<ApiResponse<bool>> SendMobileOtpAsync(SendMobileOtpRequest request)
        {
            try
            {
                // Extract 10-digit phone number (handles +91, 91, or just 10 digits)
                var phoneNumber10Digit = Extract10DigitPhoneNumber(request.PhoneNumber);

                // Validate phone number
                if (string.IsNullOrWhiteSpace(phoneNumber10Digit) || phoneNumber10Digit.Length != 10)
                    return ApiResponse<bool>.Fail("Please enter a valid 10-digit mobile number");

                // Check if user exists with this phone number (optional - during registration we don't check)
                // var existingUser = await _unitOfWork.Users.GetByPhoneAsync(phoneNumber10Digit);

                // Generate OTP (6 digits)
                var otp = GenerateOtp();

                // Store OTP in cache with 10 minutes expiry (using 10-digit number as key)
                await _cacheService.SetAsync($"mobile_otp:{phoneNumber10Digit}", otp, TimeSpan.FromMinutes(10));

                // Format full number for SMS (adds +91)
                var fullPhoneNumber = FormatPhoneNumberForSms(phoneNumber10Digit);

                // Send OTP via SMS
                await _smsService.SendOtpAsync(fullPhoneNumber, otp);

                _logger.LogInformation("Mobile OTP sent to {PhoneNumber}", phoneNumber10Digit);

                return ApiResponse<bool>.Ok(true, "OTP sent successfully to your mobile number. Valid for 10 minutes.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending mobile OTP to {PhoneNumber}", request.PhoneNumber);
                return ApiResponse<bool>.Fail("Failed to send OTP. Please try again.");
            }
        }

        /// Verify mobile number with OTP and update IsPhoneVerified flag
        public async Task<ApiResponse<bool>> VerifyMobileOtpAsync(VerifyMobileOtpRequest request)
        {
            try
            {
                // Extract 10-digit phone number (handles +91, 91, or just 10 digits)
                var phoneNumber10Digit = Extract10DigitPhoneNumber(request.PhoneNumber);

                // Validate inputs
                if (string.IsNullOrWhiteSpace(phoneNumber10Digit) || phoneNumber10Digit.Length != 10)
                    return ApiResponse<bool>.Fail("Please enter a valid 10-digit mobile number");

                if (string.IsNullOrWhiteSpace(request.Otp) || request.Otp.Length != 6)
                    return ApiResponse<bool>.Fail("Please enter a valid 6-digit OTP");

                // Get cached OTP (using 10-digit number as key)
                var cachedOtp = await _cacheService.GetAsync<string>($"mobile_otp:{phoneNumber10Digit}");

                if (cachedOtp == null)
                {
                    _logger.LogWarning("OTP expired for {PhoneNumber}", phoneNumber10Digit);
                    return ApiResponse<bool>.Fail("OTP has expired. Please request a new one.");
                }

                if (cachedOtp != request.Otp)
                {
                    _logger.LogWarning("Invalid OTP attempt for {PhoneNumber}. Expected: {Expected}, Got: {Actual}",
                        phoneNumber10Digit, cachedOtp, request.Otp);
                    return ApiResponse<bool>.Fail("Invalid OTP. Please enter the correct 6-digit OTP.");
                }

                // Find user by phone number (using 10-digit format for database lookup)
                var user = await _unitOfWork.Users.GetByPhoneAsync(phoneNumber10Digit);

                if (user == null)
                {
                    // User doesn't exist - store verified phone for registration
                    await _cacheService.SetAsync($"verified_phone:{phoneNumber10Digit}", true, TimeSpan.FromMinutes(30));
                    await _cacheService.RemoveAsync($"mobile_otp:{phoneNumber10Digit}");

                    _logger.LogInformation("Phone number {PhoneNumber} verified for new registration", phoneNumber10Digit);

                    return ApiResponse<bool>.Ok(true, "Mobile number verified successfully! You can now complete your registration.");
                }

                // User exists - mark phone as verified
                user.VerifyPhone();  // This sets IsPhoneVerified = true
                await _unitOfWork.CompleteAsync();

                // Remove OTP from cache after successful verification
                await _cacheService.RemoveAsync($"mobile_otp:{phoneNumber10Digit}");

                _logger.LogInformation("Mobile number {PhoneNumber} verified successfully for user {UserId}", phoneNumber10Digit, user.Id);

                return ApiResponse<bool>.Ok(true, "Mobile number verified successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying mobile OTP for {PhoneNumber}", request.PhoneNumber);
                return ApiResponse<bool>.Fail("Failed to verify OTP. Please try again.");
            }
        }

        /// Resend OTP to mobile number
        public async Task<ApiResponse<bool>> ResendMobileOtpAsync(ResendMobileOtpRequest request)
        {
            try
            {
                // Extract 10-digit phone number (handles +91, 91, or just 10 digits)
                var phoneNumber10Digit = Extract10DigitPhoneNumber(request.PhoneNumber);

                if (string.IsNullOrWhiteSpace(phoneNumber10Digit) || phoneNumber10Digit.Length != 10)
                    return ApiResponse<bool>.Fail("Please enter a valid 10-digit mobile number");

                // Generate new OTP
                var otp = GenerateOtp();

                // Store new OTP in cache (overwrites old one) - using 10-digit number as key
                await _cacheService.SetAsync($"mobile_otp:{phoneNumber10Digit}", otp, TimeSpan.FromMinutes(10));

                // Format full number for SMS (adds +91)
                var fullPhoneNumber = FormatPhoneNumberForSms(phoneNumber10Digit);

                // Send OTP via SMS
                await _smsService.SendOtpVerificationAsync(fullPhoneNumber, otp);

                _logger.LogInformation("Mobile OTP resent to {PhoneNumber}", phoneNumber10Digit);

                return ApiResponse<bool>.Ok(true, "OTP resent successfully. Valid for 10 minutes.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending mobile OTP to {PhoneNumber}", request.PhoneNumber);
                return ApiResponse<bool>.Fail("Failed to resend OTP. Please try again.");
            }
        }

        // ==================== Email OTP Methods ====================

        /// Send OTP to email for verification
        public async Task<ApiResponse<bool>> SendEmailOtpAsync(SendEmailOtpRequest request)
        {
            try
            {
                // Validate email
                if (string.IsNullOrWhiteSpace(request.Email))
                    return ApiResponse<bool>.Fail("Please enter a valid email address");

                // Check if user exists with this email
                var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);

                if (existingUser != null && existingUser.IsEmailVerified)
                    return ApiResponse<bool>.Fail("Email is already verified. Please login to continue.");

                // Generate OTP (6 digits)
                var otp = GenerateOtp();

                // Store OTP in cache with 10 minutes expiry
                await _cacheService.SetAsync($"email_otp:{request.Email}", otp, TimeSpan.FromMinutes(10));

                // Send OTP via Email
                await _emailService.SendVerificationEmailAsync(request.Email, otp);

                _logger.LogInformation("Email OTP sent to {Email}", request.Email);

                return ApiResponse<bool>.Ok(true, "OTP sent successfully to your email. Valid for 10 minutes.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email OTP to {Email}", request.Email);
                return ApiResponse<bool>.Fail("Failed to send OTP. Please try again.");
            }
        }
    }
}
