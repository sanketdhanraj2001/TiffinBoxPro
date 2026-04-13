using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Application.DTOs.Common;
using TiffinBox.Domain.Enums;

namespace TiffinBox.Application.DTOs.Auth
{
    public class RegisterRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public AddressDto? Address { get; set; }
    }

    public class RegisterResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public RegisterResponseDto() { }

        public RegisterResponseDto(Guid userId, string email, string message)
        {
            UserId = userId;
            Email = email;
            Message = message;
        }
    }
}
