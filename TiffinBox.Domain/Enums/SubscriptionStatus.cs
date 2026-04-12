using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Domain.Enums
{
    public enum SubscriptionStatus
    {
        Active = 1,     // Subscription is currently active
        Paused = 2,     // Temporarily paused by customer
        Cancelled = 3,  // Cancelled by customer or vendor
        Expired = 4,    // End date passed, not renewed
        Pending = 5     // Awaiting payment confirmation
    }
}
