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
    public class Payment : BaseEntity
    {
        public Guid SubscriptionId { get; private set; }
        public virtual Subscription Subscription { get; private set; }
        public Money Amount { get; private set; }
        public PaymentGateway Gateway { get; private set; }
        public string? OrderId { get; private set; }      // Gateway order ID (e.g., Razorpay order ID)
        public string? TransactionId { get; private set; } // Gateway payment ID after success
        public string? Signature { get; private set; }
        public PaymentStatus Status { get; private set; }
        public DateTime? PaidAt { get; private set; }
        public string? FailureReason { get; private set; }
        public string? RefundId { get; private set; }
        public decimal? RefundAmount { get; private set; }
        public string? ReceiptUrl { get; private set; }

        private Payment() { }

        public static Payment Create(Guid subscriptionId, Money amount, PaymentGateway gateway, string orderId, PaymentStatus status = PaymentStatus.Pending)
        {
            return new Payment
            {
                SubscriptionId = subscriptionId,
                Amount = amount,
                Gateway = gateway,
                OrderId = orderId,
                Status = status
            };
        }

        public void MarkCompleted(string transactionId, string signature)
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidOperationException("Only pending payments can be marked as completed");

            Status = PaymentStatus.Completed;
            TransactionId = transactionId;
            Signature = signature;
            PaidAt = DateTime.UtcNow;
            UpdateTimestamp();
        }

        public void MarkFailed(string reason)
        {
            if (Status != PaymentStatus.Pending)
                throw new InvalidOperationException("Only pending payments can be marked as failed");

            Status = PaymentStatus.Failed;
            FailureReason = reason;
            UpdateTimestamp();
        }

        public void Refund(decimal amount, string refundId)
        {
            if (Status != PaymentStatus.Completed)
                throw new InvalidOperationException("Only completed payments can be refunded");

            if (amount > Amount.Amount)
                throw new ArgumentException("Refund amount cannot exceed original amount");

            if (amount == Amount.Amount)
                Status = PaymentStatus.Refunded;
            else
                Status = PaymentStatus.PartialRefunded;

            RefundId = refundId;
            RefundAmount = amount;
            UpdateTimestamp();
        }

        public bool IsSuccessful() => Status == PaymentStatus.Completed;
        public bool IsRefunded() => Status == PaymentStatus.Refunded || Status == PaymentStatus.PartialRefunded;
    }
}
