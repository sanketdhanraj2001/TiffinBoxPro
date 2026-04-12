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
                // ✅ Fix: Use request.Email (make sure LoginRequestDto has Email property)
                var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt failed: User not found - {Email}", request.Email);
                    return ApiResponse<LoginResponseDto>.Fail("Invalid email or password");
                }

                // Check if user is locked out
                if (user.IsLockedOut())
                {
                    return ApiResponse<LoginResponseDto>.Fail("Account is locked out. Please try again later.");
                }

                // ✅ Fix: Use request.Password
                if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    user.RecordLoginFailure();
                    await _unitOfWork.CompleteAsync();

                    _logger.LogWarning("Login attempt failed: Invalid password - {Email}", request.Email);
                    return ApiResponse<LoginResponseDto>.Fail("Invalid email or password");
                }

                // Check if email is verified
                if (!user.IsEmailVerified)
                {
                    return ApiResponse<LoginResponseDto>.Fail("Please verify your email before logging in");
                }

                // Check if account is active
                if (!user.IsActive)
                {
                    return ApiResponse<LoginResponseDto>.Fail("Your account has been deactivated");
                }

                // Record successful login
                user.RecordLoginSuccess();

                // Generate tokens
                var (accessToken, expiresAt) = GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken();

                user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays));
                await _unitOfWork.CompleteAsync();

                var userProfile = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role.ToString(),
                    IsEmailVerified = user.IsEmailVerified,
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    VendorId = user.Vendor?.Id,
                    DeliveryAgentId = user.DeliveryAgent?.Id
                };

                var response = new LoginResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt,
                    User = userProfile
                };

                _logger.LogInformation("User logged in successfully: {UserId} - {Email}", user.Id, user.Email);

                return ApiResponse<LoginResponseDto>.Ok(response, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return ApiResponse<LoginResponseDto>.Fail("An error occurred during login");
            }
        }

        private (string Token, DateTime ExpiresAt) GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (user.Vendor != null)
            {
                claims.Add(new Claim("vendor_id", user.Vendor.Id.ToString()));
            }

            if (user.DeliveryAgent != null)
            {
                claims.Add(new Claim("agent_id", user.DeliveryAgent.Id.ToString()));
            }

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

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
