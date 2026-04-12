using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.DTOs.Common
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Role { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public Guid? VendorId { get; set; }
        public Guid? DeliveryAgentId { get; set; }
        public decimal WalletBalance { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
