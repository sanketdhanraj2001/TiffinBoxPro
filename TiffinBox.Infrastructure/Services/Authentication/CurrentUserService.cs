using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Application.Common.Interfaces;

namespace TiffinBox.Infrastructure.Services.Authentication
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
                throw new Exception("Invalid or missing user id");

            return userId;
        }

        public string? GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.Role)?.Value;
        }

        public string? GetUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.Email)?.Value;
        }

        public string? GetUserPhone()
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.MobilePhone)?.Value;
        }

        public string? GetUserFullName()
        {
            var firstName = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.GivenName)?.Value;

            var lastName = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.Surname)?.Value;

            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
                return null;

            return $"{firstName} {lastName}".Trim();
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?
                .Identity?.IsAuthenticated ?? false;
        }

        public bool IsInRole(string role)
        {
            return _httpContextAccessor.HttpContext?.User?
                .IsInRole(role) ?? false;
        }
    }
}
