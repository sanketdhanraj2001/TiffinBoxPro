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
        public int UserId { get; private set; }
        public virtual User User { get; private set; }
        public Money Balance { get; private set; }
        public bool IsActive { get; private set; }
        public virtual ICollection<WalletTransaction> Transactions { get; private set; } = new List<WalletTransaction>();

        private Wallet() { }

        public static Wallet Create(int userId)
        {
            return new Wallet
            {
                UserId = userId,
                Balance = new Money(0, "INR"),
                IsActive = true
            };
        }

       
        public void Credit(decimal amount, string description, string? referenceId = null)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive");

            var creditAmount = new Money(amount, Balance.Currency);
            Balance = new Money(Balance.Amount + creditAmount.Amount, Balance.Currency);

            var transaction = WalletTransaction.CreateCredit(Id, creditAmount, description, referenceId, Balance.Amount);
            Transactions.Add(transaction);
            UpdateTimestamp();
        }

        public void Debit(decimal amount, string description, string? referenceId = null)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive");
            if (Balance.Amount < amount) throw new InvalidOperationException("Insufficient balance");

            var debitAmount = new Money(amount, Balance.Currency);
            Balance = new Money(Balance.Amount - debitAmount.Amount, Balance.Currency);

            var transaction = WalletTransaction.CreateDebit(Id, debitAmount, description, referenceId, Balance.Amount);
            Transactions.Add(transaction);
            UpdateTimestamp();
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
    }
}
