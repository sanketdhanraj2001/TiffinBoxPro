using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.DTOs.Customer
{
    public class WalletDto
    {
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "INR";
        public List<WalletTransactionDto> Transactions { get; set; } = new();
    }

    public class WalletTransactionDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;  // Credit or Debit
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? ReferenceId { get; set; }  // OrderId, PaymentId, etc.
        public string Status { get; set; } = "Completed";
    }
}
