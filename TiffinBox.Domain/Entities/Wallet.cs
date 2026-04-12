using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Common;
using TiffinBox.Domain.ValueObjects;

namespace TiffinBox.Domain.Entities
{
    public class Wallet : BaseEntity
    {
        public Guid UserId { get; private set; }
        public virtual User User { get; private set; }
        public Money Balance { get; private set; }
        public bool IsActive { get; private set; }

        // Navigation property
        public virtual ICollection<WalletTransaction> Transactions { get; private set; } = new List<WalletTransaction>();

        private Wallet() { }

        public static Wallet Create(string userEmail)
        {
            return new Wallet
            {
                UserId = Guid.Empty, // Will be set when attached to user
                Balance = new Money(0, "INR"),
                IsActive = true
            };
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

        public void Credit(decimal amount, string currency, string description, string? referenceId = null)
        {
            if (!IsActive) throw new InvalidOperationException("Wallet is deactivated");
            if (amount <= 0) throw new ArgumentException("Amount must be positive");

            var creditAmount = new Money(amount, currency);
            Balance += creditAmount;

            var transaction = WalletTransaction.CreateCredit(this.Id, creditAmount, description, referenceId);
            Transactions.Add(transaction);

            UpdateTimestamp();
        }

        public void Debit(decimal amount, string currency, string description, string? referenceId = null)
        {
            if (!IsActive) throw new InvalidOperationException("Wallet is deactivated");
            if (amount <= 0) throw new ArgumentException("Amount must be positive");

            var debitAmount = new Money(amount, currency);
            if (Balance.Amount < debitAmount.Amount)
                throw new InvalidOperationException("Insufficient balance");

            Balance -= debitAmount;

            var transaction = WalletTransaction.CreateDebit(this.Id, debitAmount, description, referenceId);
            Transactions.Add(transaction);

            UpdateTimestamp();
        }

        public decimal GetAvailableBalance() => Balance.Amount;
    }
}
