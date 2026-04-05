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
    public class Order : BaseEntity
    {
        public Guid SubscriptionId { get; private set; }
        public virtual Subscription Subscription { get; private set; }
        public Guid? DeliveryAgentId { get; private set; }
        public virtual DeliveryAgent? DeliveryAgent { get; private set; }
        public DateTime DeliveryDate { get; private set; }
        public OrderStatus Status { get; private set; }
        public Money TotalAmount { get; private set; }
        public string? TrackingUrl { get; private set; }
        public string? DeliveryInstructions { get; private set; }
        public DateTime? ConfirmedAt { get; private set; }
        public DateTime? PreparedAt { get; private set; }
        public DateTime? OutForDeliveryAt { get; private set; }
        public DateTime? DeliveredAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public string? CancellationReason { get; private set; }

        // Navigation properties
        public virtual ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

        private Order() { }

        public static Order Create(Guid subscriptionId, DateTime deliveryDate, Money totalAmount, string? instructions = null)
        {
            return new Order
            {
                SubscriptionId = subscriptionId,
                DeliveryDate = deliveryDate,
                Status = OrderStatus.Pending,
                TotalAmount = totalAmount,
                DeliveryInstructions = instructions
            };
        }

        public void Confirm()
        {
            if (Status != OrderStatus.Pending)
                throw new BusinessRuleViolationException("Only pending orders can be confirmed");

            Status = OrderStatus.Confirmed;
            ConfirmedAt = DateTime.UtcNow;
            UpdateTimestamp();
        }

        public void StartPreparation()
        {
            if (Status != OrderStatus.Confirmed)
                throw new BusinessRuleViolationException("Only confirmed orders can be prepared");

            Status = OrderStatus.Preparing;
            PreparedAt = DateTime.UtcNow;
            UpdateTimestamp();
        }

        public void AssignDeliveryAgent(Guid agentId)
        {
            if (Status != OrderStatus.Preparing)
                throw new BusinessRuleViolationException("Only preparing orders can be assigned for delivery");

            DeliveryAgentId = agentId;
            Status = OrderStatus.OutForDelivery;
            OutForDeliveryAt = DateTime.UtcNow;
            UpdateTimestamp();
        }

        public void MarkAsDelivered()
        {
            if (Status != OrderStatus.OutForDelivery)
                throw new BusinessRuleViolationException("Only orders out for delivery can be marked as delivered");

            Status = OrderStatus.Delivered;
            DeliveredAt = DateTime.UtcNow;
            UpdateTimestamp();
        }

        public void Cancel(string reason, UserRole cancelledBy)
        {
            if (Status == OrderStatus.Delivered)
                throw new BusinessRuleViolationException("Delivered orders cannot be cancelled");

            Status = OrderStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            CancellationReason = reason;
            UpdateTimestamp();
        }

        public void UpdateTrackingUrl(string url)
        {
            TrackingUrl = url;
            UpdateTimestamp();
        }

        public void AddOrderItem(OrderItem item)
        {
            OrderItems.Add(item);
            UpdateTimestamp();
        }
    }
}
