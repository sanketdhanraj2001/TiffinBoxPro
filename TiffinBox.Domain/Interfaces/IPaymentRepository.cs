using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Entities;

namespace TiffinBox.Domain.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<Payment?> GetByTransactionIdAsync(string transactionId);
        Task<Payment?> GetByOrderIdAsync(string orderId);
        Task<IReadOnlyList<Payment>> GetPaymentsBySubscriptionAsync(Guid subscriptionId);
        Task<IReadOnlyList<Payment>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to);
        Task<decimal> GetTotalCollectedAsync(DateTime from, DateTime to);
    }
}
