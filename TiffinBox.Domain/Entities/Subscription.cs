using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Common;
using TiffinBox.Domain.Enums;
using TiffinBox.Domain.Exceptions;
using TiffinBox.Domain.ValueObjects;

namespace TiffinBox.Domain.Entities
{
    public class Subscription : BaseEntity
    {
        public Guid CustomerId { get; private set; }
        public virtual User Customer { get; private set; }
        public Guid VendorId { get; private set; }
        public virtual Vendor Vendor { get; private set; }  
        public Guid PlanId { get; private set; }
        public SubscriptionPlan Plan { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public SubscriptionStatus Status { get; private set; }
        public DateTime? CancelledAt { get; private set; }

        public string? CancellationReason { get; private set; }
        public Money TotalAmount { get; private set; }
        public int TotalDays { get; private set; }
        public int DeliveredDays { get; private set; }
        public int SkippedDays { get; private set; }
        public ICollection<SubscriptionMeal> Meals { get; private set; } = new List<SubscriptionMeal>();
        public ICollection<Order> Orders { get; private set; } = new List<Order>();

        private Subscription() { }

        public static Subscription Create(
            Guid customerId,
            SubscriptionPlan plan,
            DateTime startDate,
            List<MealSelection> mealSelections)
        {
            var subscription = new Subscription
            {
                CustomerId = customerId,
                VendorId = plan.VendorId,
                PlanId = plan.Id,
                Plan = plan,
                StartDate = startDate,
                EndDate = startDate.AddDays(plan.DurationDays),
                Status = SubscriptionStatus.Active,
                TotalAmount = plan.Price,
                TotalDays = plan.DurationDays
            };

            foreach (var selection in mealSelections)
            {
                subscription.Meals.Add(SubscriptionMeal.Create(
                    subscription.Id,
                    selection.MenuItemId,
                    selection.DaysOfWeek));
            }

            return subscription;
        }

        public void Pause(DateTime pauseStart, DateTime pauseEnd)
        {
            if (Status != SubscriptionStatus.Active)
                throw new InvalidOperationException("Only active subscriptions can be paused");

            Status = SubscriptionStatus.Paused;
            // Additional logic for pausing specific days
        }

        public void Resume()
        {
            if (Status != SubscriptionStatus.Paused)
                throw new InvalidOperationException("Only paused subscriptions can be resumed");

            Status = SubscriptionStatus.Active;
        }

        public void Cancel()
        {
            if (Status == SubscriptionStatus.Cancelled)
                throw new InvalidOperationException("Subscription already cancelled");

            Status = SubscriptionStatus.Cancelled;
        }
        public void Cancel(string reason)
        {
            if (Status == SubscriptionStatus.Cancelled)
                throw new BusinessRuleViolationException("Subscription is already cancelled");

            if (Status == SubscriptionStatus.Expired)
                throw new BusinessRuleViolationException("Cannot cancel an expired subscription");

            if (Status != SubscriptionStatus.Active && Status != SubscriptionStatus.Paused)
                throw new BusinessRuleViolationException($"Cannot cancel subscription with status: {Status}");

            Status = SubscriptionStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            CancellationReason = reason;
            UpdateTimestamp();
        }

        public void RecordDelivery(DateTime date)
        {
            DeliveredDays++;
            UpdateTimestamp();
        }

        public void RecordSkip(DateTime date)
        {
            SkippedDays++;
            UpdateTimestamp();
        }

        public bool IsActiveOnDate(DateTime date)
        {
            return Status == SubscriptionStatus.Active
                   && date >= StartDate
                   && date <= EndDate
                   && !IsHoliday(date);
        }

        private bool IsHoliday(DateTime date)
        {
            // Check if date is in vendor's holiday list or customer's skip days
            return false;
        }
    }
}
