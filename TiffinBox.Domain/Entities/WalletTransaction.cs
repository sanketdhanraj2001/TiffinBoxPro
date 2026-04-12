using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TiffinBox.Domain.Common;
using TiffinBox.Domain.Enums;
using TiffinBox.Domain.ValueObjects;
using TransactionStatus = TiffinBox.Domain.Enums.TransactionStatus;

namespace TiffinBox.Domain.Entities
{
    public class WalletTransaction: BaseEntity
    {
        public Guid WalletId { get; private set; }
        public virtual Wallet Wallet { get; private set; }
        public TransactionType Type { get; private set; }  // Credit or Debit
        public Money Amount { get; private set; }
        public decimal BalanceAfter { get; private set; }
        public string Description { get; private set; }
        public string? ReferenceId { get; private set; }   // OrderId, PaymentId, etc.
        public TransactionStatus Status { get; private set; }

        private WalletTransaction() { }

        public static WalletTransaction CreateCredit(Guid walletId, Money amount, string description, string? referenceId = null)
        {
            return new WalletTransaction
            {
                WalletId = walletId,
                Type = TransactionType.Credit,
                Amount = amount,
                Description = description,
                ReferenceId = referenceId,
                Status = TransactionStatus.Completed
            };
        }

        public static WalletTransaction CreateDebit(Guid walletId, Money amount, string description, string? referenceId = null)
        {
            return new WalletTransaction
            {
                WalletId = walletId,
                Type = TransactionType.Debit,
                Amount = amount,
                Description = description,
                ReferenceId = referenceId,
                Status = TransactionStatus.Completed
            };
        }

        public void MarkAsPending()
        {
            Status = TransactionStatus.Pending;
            UpdateTimestamp();
        }

        public void MarkAsCompleted()
        {
            Status = TransactionStatus.Completed;
            UpdateTimestamp();
        }

        public void MarkAsFailed()
        {
            Status = TransactionStatus.Failed;
            UpdateTimestamp();
        }
    }
}
