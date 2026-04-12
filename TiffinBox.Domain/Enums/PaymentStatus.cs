using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 1,    // Created but not completed
        Completed = 2,  // Successfully charged
        Failed = 3,     // Payment failed
        Refunded = 4,   // Fully refunded
        PartialRefunded = 5 // Partially refunded
    }


}
