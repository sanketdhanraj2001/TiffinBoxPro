using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Common;
using TiffinBox.Domain.Enums;

namespace TiffinBox.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; private set; }
        public string PhoneNumber { get; private set; }
        public string PasswordHash { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string FullName => $"{FirstName} {LastName}";
        public UserRole Role { get; private set; }
        public bool IsEmailVerified { get; private set; }
        public bool IsPhoneVerified { get; private set; }
        public bool IsActive { get; private set; }
        public Address? Address { get; private set; }
        public string? ProfilePictureUrl { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public int FailedLoginAttempts { get; private set; }
        public DateTime? LockoutEnd { get; private set; }
        public string? RefreshToken { get; private set; }
        public DateTime? RefreshTokenExpiryTime { get; private set; }

        // Navigation properties
        public virtual Vendor? Vendor { get; private set; }
        public virtual DeliveryAgent? DeliveryAgent { get; private set; }
        public virtual ICollection<Review> Reviews { get; private set; } = new List<Review>();
        public virtual Wallet Wallet { get; private set; }

        private User() { }

        public static User Create(string email, string phoneNumber, string firstName, string lastName, UserRole role)
        {
            return new User
            {
                Email = email.ToLowerInvariant(),
                PhoneNumber = phoneNumber,
                FirstName = firstName,
                LastName = lastName,
                Role = role,
                IsActive = true,
                FailedLoginAttempts = 0,
                Wallet = Wallet.Create(email)
            };
        }

        public void SetPasswordHash(string hash) => PasswordHash = hash;

        public void VerifyEmail()
        {
            IsEmailVerified = true;
            UpdateTimestamp();
        }

        public void VerifyPhone()
        {
            IsPhoneVerified = true;
            UpdateTimestamp();
        }

        public void RecordLoginSuccess()
        {
            LastLoginAt = DateTime.UtcNow;
            FailedLoginAttempts = 0;
            LockoutEnd = null;
            UpdateTimestamp();
        }

        public void RecordLoginFailure()
        {
            FailedLoginAttempts++;
            UpdateTimestamp();

            if (FailedLoginAttempts >= 5)
            {
                LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            }
        }

        public bool IsLockedOut() => LockoutEnd.HasValue && LockoutEnd > DateTime.UtcNow;

        public void UpdateProfile(string firstName, string lastName, Address? address, string? profilePictureUrl)
        {
            FirstName = firstName;
            LastName = lastName;
            Address = address;
            ProfilePictureUrl = profilePictureUrl;
            UpdateTimestamp();
        }

        public void SetRefreshToken(string refreshToken, DateTime expiryTime)
        {
            RefreshToken = refreshToken;
            RefreshTokenExpiryTime = expiryTime;
            UpdateTimestamp();
        }

        public void RevokeRefreshToken()
        {
            RefreshToken = null;
            RefreshTokenExpiryTime = null;
            UpdateTimestamp();
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdateTimestamp();
        }

        public void Activate()
        {
            IsActive = true;
            UpdateTimestamp();
        }
    }
}
