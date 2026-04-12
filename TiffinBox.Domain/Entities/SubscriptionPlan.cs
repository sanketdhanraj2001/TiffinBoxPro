using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Common;
using TiffinBox.Domain.Enums;
using TiffinBox.Domain.ValueObjects;

namespace TiffinBox.Domain.Entities
{
    public class SubscriptionPlan : BaseEntity
    {
        public Guid VendorId { get; private set; }
        public virtual Vendor Vendor { get; private set; }
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public PlanType PlanType { get; private set; }
        public Money Price { get; private set; }
        public int DurationDays { get; private set; }
        public int MealsPerDay { get; private set; }
        public bool IsActive { get; private set; }
        public int MaxSubscribers { get; private set; }
        public int CurrentSubscribers { get; private set; }
        public List<DayOfWeek> DeliveryDays { get; private set; } = new();

        // Navigation properties
        public virtual ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();

        private SubscriptionPlan() { }

        public static SubscriptionPlan Create(
            Guid vendorId,
            string name,
            PlanType planType,
            Money price,
            int durationDays,
            int mealsPerDay,
            List<DayOfWeek> deliveryDays)
        {
            return new SubscriptionPlan
            {
                VendorId = vendorId,
                Name = name,
                PlanType = planType,
                Price = price,
                DurationDays = durationDays,
                MealsPerDay = mealsPerDay,
                DeliveryDays = deliveryDays,
                IsActive = true,
                MaxSubscribers = 50
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
            UpdateTimestamp();
        }

        public bool HasCapacity()
        {
            return CurrentSubscribers < MaxSubscribers;
        }

        public void IncrementSubscribers()
        {
            CurrentSubscribers++;
            UpdateTimestamp();
        }

        public void DecrementSubscribers()
        {
            CurrentSubscribers--;
            UpdateTimestamp();
        }

        public void UpdatePrice(Money newPrice)
        {
            Price = newPrice;
            UpdateTimestamp();
        }
    }
}
