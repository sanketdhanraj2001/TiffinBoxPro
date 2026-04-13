using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Common;

namespace TiffinBox.Domain.ValueObjects
{
    public class Money : ValueObject
    {
        public int Id { get; set; }
        public decimal Amount { get; init; }
        public string Currency { get; init; }

        private Money()
        {
            Amount = 0;
            Currency = "INR";
        }

        public Money(decimal amount, string currency = "INR")
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative");
            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Currency is required");

            Amount = Math.Round(amount, 2);
            Currency = currency.ToUpperInvariant();
        }

        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException($"Cannot add {a.Currency} with {b.Currency}");

            return new Money(a.Amount + b.Amount, a.Currency);
        }

        public static Money operator -(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException($"Cannot subtract {a.Currency} with {b.Currency}");

            return new Money(a.Amount - b.Amount, a.Currency);
        }

        public static Money operator *(Money a, decimal multiplier)
        {
            return new Money(a.Amount * multiplier, a.Currency);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }

        public override string ToString() => $"{Currency} {Amount:F2}";
    }
}
