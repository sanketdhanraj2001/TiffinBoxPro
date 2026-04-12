using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Domain.Enums
{
    public enum TransactionType
    {
        Credit = 1,
        Debit = 2
    }
    public enum TransactionStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4
    }
    public enum NotificationType
    {
        None = 0,
        OrderPlaced = 1,
        OrderConfirmed = 2,
        OrderPreparing = 3,
        OrderOutForDelivery = 4,
        OrderDelivered = 5,
        OrderCancelled = 6,
        SubscriptionCreated = 7,
        SubscriptionCancelled = 8,
        SubscriptionPaused = 9,
        SubscriptionResumed = 10,
        SubscriptionExpiring = 11,
        PaymentSuccess = 12,
        PaymentFailed = 13,
        PaymentRefunded = 14,
        WalletCredited = 15,
        WalletDebited = 16,
        WalletLowBalance = 17,
        VendorApproved = 18,
        VendorRejected = 19,
        VendorSuspended = 20,
        DeliveryAssigned = 21,
        DeliveryUpdated = 22,
        Promotional = 23,
        SystemAlert = 24,
        Welcome = 25,
        EmailVerification = 26,
        PasswordReset = 27
    }

    public enum NotificationChannel
    {
        InApp = 1,
        Email = 2,
        SMS = 3,
        Push = 4,
        All = 5
    }
}
