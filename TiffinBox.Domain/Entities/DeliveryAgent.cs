using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Common;
using TiffinBox.Domain.ValueObjects;

namespace TiffinBox.Domain.Entities
{
    public class DeliveryAgent : BaseEntity
    {
        public Guid UserId { get; private set; }
        public virtual User User { get; private set; }
        public Guid VendorId { get; private set; }
        public virtual Vendor Vendor { get; private set; }
        public string? VehicleNumber { get; private set; }
        public string? VehicleType { get; private set; }  // Bike, Scooter, Car
        public bool IsActive { get; private set; }
        public bool IsAvailable { get; private set; }
        public GeoLocation? CurrentLocation { get; private set; }
        public double Rating { get; private set; }
        public int TotalRatings { get; private set; }
        public int TotalDeliveries { get; private set; }
        public DateTime? LastDeliveryAt { get; private set; }

        // Navigation properties
        public virtual ICollection<Order> Orders { get; private set; } = new List<Order>();

        private DeliveryAgent() { }

        public static DeliveryAgent Create(User user, Guid vendorId, string? vehicleNumber = null, string? vehicleType = null)
        {
            return new DeliveryAgent
            {
                UserId = user.Id,
                User = user,
                VendorId = vendorId,
                VehicleNumber = vehicleNumber,
                VehicleType = vehicleType,
                IsActive = true,
                IsAvailable = true,
                Rating = 0,
                TotalRatings = 0,
                TotalDeliveries = 0
            };
        }

        public void Activate()
        {
            IsActive = true;
            UpdateTimestamp();
        }

        public void Deactivate()
        {
            IsActive = false;
            IsAvailable = false;
            UpdateTimestamp();
        }

        public void SetAvailability(bool isAvailable)
        {
            IsAvailable = isAvailable;
            UpdateTimestamp();
        }

        public void UpdateLocation(GeoLocation location)
        {
            CurrentLocation = location;
            UpdateTimestamp();
        }

        public void UpdateVehicleInfo(string? vehicleNumber, string? vehicleType)
        {
            if (!string.IsNullOrWhiteSpace(vehicleNumber))
                VehicleNumber = vehicleNumber;

            if (!string.IsNullOrWhiteSpace(vehicleType))
                VehicleType = vehicleType;

            UpdateTimestamp();
        }

        public void RecordDelivery()
        {
            TotalDeliveries++;
            LastDeliveryAt = DateTime.UtcNow;
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
            return IsActive && IsAvailable;
        }
    }
}
