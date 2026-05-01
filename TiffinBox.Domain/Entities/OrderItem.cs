using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Common;
using TiffinBox.Domain.ValueObjects;

namespace TiffinBox.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; private set; }
        public virtual Order Order { get; private set; }
        public int MenuItemId { get; private set; }
        public virtual MenuItem MenuItem { get; private set; }
        public int Quantity { get; private set; }
        public Money UnitPrice { get; private set; }
        public Money TotalPrice => new Money(UnitPrice.Amount * Quantity, UnitPrice.Currency);
        public string? SpecialInstructions { get; private set; }

        private OrderItem() { }

        public static OrderItem Create(int orderId, int menuItemId, int quantity, Money unitPrice, string? instructions = null)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");

            if (unitPrice.Amount <= 0)
                throw new ArgumentException("Unit price must be greater than zero");

            return new OrderItem
            {
                OrderId = orderId,
                MenuItemId = menuItemId,
                Quantity = quantity,
                UnitPrice = unitPrice,
                SpecialInstructions = instructions
            };
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero");

            Quantity = newQuantity;
            UpdateTimestamp();
        }

        public void UpdateSpecialInstructions(string? instructions)
        {
            SpecialInstructions = instructions;
            UpdateTimestamp();
        }
    }
}
