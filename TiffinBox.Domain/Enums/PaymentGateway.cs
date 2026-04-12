using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Domain.Enums
{
    public enum PaymentGateway
    {
        None = 0,
        Razorpay = 1,
        Stripe = 2,
        Cash = 3,
        Wallet = 4
    }
}
