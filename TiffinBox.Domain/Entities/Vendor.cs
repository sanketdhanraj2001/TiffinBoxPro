using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Common;
using TiffinBox.Domain.ValueObjects;

namespace TiffinBox.Domain.Entities
{
    public class Vendor : BaseEntity
    {
        public int UserId { get; private set; }
        public virtual User User { get; private set; }
        public string BusinessName { get; private set; }
        public string? GSTIN { get; private set; }
        public string? FSSAILicense { get; private set; }
        public string? Description { get; private set; }
        public string? LogoUrl { get; private set; }
        public string? CoverImageUrl { get; private set; }

        [ForeignKey(nameof(BusinessAddress))]
        public int BusinessAddressId { get; private set; }
        public Address BusinessAddress { get; private set; }
        public GeoLocation? Location { get; private set; }
        public double Rating { get; private set; }
        public int TotalRatings { get; private set; }
        public bool IsApproved { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public string? ApprovedBy { get; private set; }
        public int MaxDailyOrders { get; private set; }
        public int CurrentDailyOrders { get; private set; }
        public TimeOnly OpeningTime { get; private set; }
        public TimeOnly ClosingTime { get; private set; }
        public List<string> ServiceAreas { get; private set; } = new();


        //  FIXED: Changed from string? to ICollection<Review>
        public virtual ICollection<Review> Reviews { get; private set; } = new List<Review>();
        //  ADDED: Cuisine type (e.g., North Indian, Chinese, Italian)
        public string? Cuisine { get; private set; }

        // ADDED: Delivery fee and minimum order amount
        public Money? DeliveryFee { get; private set; }
        public Money? MinOrderAmount { get; private set; }

        // ADDED: Estimated delivery time in minutes
        public int EstimatedDeliveryTime { get; private set; } = 30;


        // Navigation properties
        public virtual ICollection<MenuItem> MenuItems { get; private set; } = new List<MenuItem>();
        public virtual ICollection<SubscriptionPlan> SubscriptionPlans { get; private set; } = new List<SubscriptionPlan>();
        public virtual ICollection<DeliveryAgent> DeliveryAgents { get; private set; } = new List<DeliveryAgent>();
        public virtual ICollection<Order> Orders { get; private set; } = new List<Order>();

        private Vendor() { }

        public static Vendor Create(User user, string businessName, Address businessAddress, string? gstin = null)
        {
            return new Vendor
            {
                UserId = user.Id,
                User = user,
                BusinessName = businessName,
                BusinessAddress = businessAddress,
                GSTIN = gstin,
                IsApproved = false,
                IsActive = true,
                MaxDailyOrders = 100,
                Rating = 0,
                TotalRatings = 0,
                OpeningTime = new TimeOnly(8, 0),
                ClosingTime = new TimeOnly(20, 0)
            };
        }

        public void Approve(string approvedBy)
        {
            IsApproved = true;
            ApprovedAt = DateTime.UtcNow;
            ApprovedBy = approvedBy;
            UpdateTimestamp();
        }

        public void Reject()
        {
            IsApproved = false;
            UpdateTimestamp();
        }

        public void Suspend()
        {
            IsActive = false;
            UpdateTimestamp();
        }

        public void Activate()
        {
            IsActive = true;
            UpdateTimestamp();
        }

        public void UpdateRating(double newRating)
        {
            Rating = newRating;
            UpdateTimestamp();
        }

        public void AddRating(int rating)
        {
            var total = (Rating * TotalRatings) + rating;
            TotalRatings++;
            Rating = total / TotalRatings;
            UpdateTimestamp();
        }

        public bool CanAcceptOrder()
        {
            return IsApproved && IsActive && CurrentDailyOrders < MaxDailyOrders;
        }

        public void IncrementDailyOrders()
        {
            CurrentDailyOrders++;
            UpdateTimestamp();
        }

        public void DecrementDailyOrders()
        {
            CurrentDailyOrders--;
            UpdateTimestamp();
        }

        public void UpdateBusinessHours(TimeOnly opening, TimeOnly closing)
        {
            OpeningTime = opening;
            ClosingTime = closing;
            UpdateTimestamp();
        }

        public void UpdateServiceAreas(List<string> areas)
        {
            ServiceAreas = areas;
            UpdateTimestamp();
        }
    }
}
