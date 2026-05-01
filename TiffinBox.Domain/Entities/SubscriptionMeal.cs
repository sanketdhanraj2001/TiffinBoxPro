using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Common;

namespace TiffinBox.Domain.Entities
{
    public class SubscriptionMeal : BaseEntity
    {
        public int SubscriptionId { get; private set; }
        public virtual Subscription Subscription { get; private set; }
        public int MenuItemId { get; private set; }
        public virtual MenuItem MenuItem { get; private set; }
        public List<DayOfWeek> DaysOfWeek { get; private set; } = new();
        public bool IsActive { get; private set; }

        private SubscriptionMeal() { }

        public static SubscriptionMeal Create(int subscriptionId, int menuItemId, List<DayOfWeek> daysOfWeek)
        {
            if (daysOfWeek == null || !daysOfWeek.Any())
                throw new ArgumentException("At least one day of week must be selected");

            return new SubscriptionMeal
            {
                SubscriptionId = subscriptionId,
                MenuItemId = menuItemId,
                DaysOfWeek = daysOfWeek,
                IsActive = true
            };
        }

        public void UpdateDaysOfWeek(List<DayOfWeek> daysOfWeek)
        {
            if (daysOfWeek == null || !daysOfWeek.Any())
                throw new ArgumentException("At least one day of week must be selected");

            DaysOfWeek = daysOfWeek;
            UpdateTimestamp();
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

        public bool IsSelectedForDay(DayOfWeek day)
        {
            return IsActive && DaysOfWeek.Contains(day);
        }
    }
}
