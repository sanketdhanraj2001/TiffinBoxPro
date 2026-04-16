using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TiffinBox.Application.Common.Interfaces;
using TiffinBox.Application.Common.Models;
using TiffinBox.Application.DTOs.Auth;

namespace TiffinBox.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _authService.RegisterAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var result = await _authService.VerifyOtpAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
        {
            var result = await _authService.ResendVerificationOtpAsync(request.Email);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var result = await _authService.ForgotPasswordAsync(request);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _authService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _authService.GetCurrentUserAsync(userId);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _authService.UpdateProfileAsync(userId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _authService.LogoutAsync(userId);
            return Ok(result);
        }

        // ==================== Mobile OTP Endpoints ====================

        /// Send OTP to mobile number for verification
        [HttpPost("send-mobile-otp")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> SendMobileOtp([FromBody] SendMobileOtpRequest request)
        {
            var result = await _authService.SendMobileOtpAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// Verify mobile number with OTP
        [HttpPost("verify-mobile-otp")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> VerifyMobileOtp([FromBody] VerifyMobileOtpRequest request)
        {
            var result = await _authService.VerifyMobileOtpAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// Resend OTP to mobile number
        [HttpPost("resend-mobile-otp")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public async Task<IActionResult> ResendMobileOtp([FromBody] ResendMobileOtpRequest request)
        {
            var result = await _authService.ResendMobileOtpAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ==================== Email OTP Endpoints ====================

        [HttpPost("send-email-otp")]
        public async Task<IActionResult> SendEmailOtp([FromBody] SendEmailOtpRequest request)
        {
            var result = await _authService.SendEmailOtpAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("verify-email-otp")]
        public async Task<IActionResult> VerifyEmailOtp([FromBody] VerifyOtpRequest request)
        {
            var result = await _authService.VerifyOtpAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("resend-email-otp")]
        public async Task<IActionResult> ResendEmailOtp([FromBody] ResendOtpRequest request)
        {
            var result = await _authService.ResendVerificationOtpAsync(request.Email);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
