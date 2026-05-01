using Microsoft.AspNetCore.Identity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Application.Common.Models;
using TiffinBox.Application.DTOs.Auth;
using TiffinBox.Application.DTOs.Common;

namespace TiffinBox.Application.Common.Interfaces
{
    public interface IAuthenticationService
    {

        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<ApiResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request);
        Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<ApiResponse<bool>> LogoutAsync(int userId);
        Task<ApiResponse<UserProfileDto>> GetCurrentUserAsync(int userId);
        Task<ApiResponse<bool>> ForgotPasswordAsync(DTOs.Auth.ForgotPasswordRequest request);
        Task<ApiResponse<bool>> ResetPasswordAsync(DTOs.Auth.ResetPasswordRequest request);
        Task<ApiResponse<bool>> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
        Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileRequest request);

        //  Mobile OTP Methods
        Task<ApiResponse<bool>> SendMobileOtpAsync(SendMobileOtpRequest request);
        Task<ApiResponse<bool>> VerifyMobileOtpAsync(VerifyMobileOtpRequest request);
        Task<ApiResponse<bool>> ResendMobileOtpAsync(ResendMobileOtpRequest request);



        // Email OTP Methods
        Task<ApiResponse<bool>> VerifyOtpAsync(VerifyOtpRequest request);
        Task<ApiResponse<bool>> ResendVerificationOtpAsync(string email);
        Task<ApiResponse<bool>> SendEmailOtpAsync(SendEmailOtpRequest request);

       
    }
}
