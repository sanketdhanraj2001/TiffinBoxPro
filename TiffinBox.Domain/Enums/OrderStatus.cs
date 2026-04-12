using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,        // Order placed, awaiting vendor confirmation
        Confirmed = 2,      // Vendor accepted
        Preparing = 3,      // Meal being prepared
        OutForDelivery = 4, // Assigned to delivery agent
        Delivered = 5,      // Customer received
        Cancelled = 6,      // Cancelled by customer or vendor
        Failed = 7          // Delivery failed
    }
}
