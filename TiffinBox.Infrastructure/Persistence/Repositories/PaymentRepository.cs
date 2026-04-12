using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Entities;
using TiffinBox.Domain.Enums;
using TiffinBox.Domain.Interfaces;

namespace TiffinBox.Infrastructure.Persistence.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
            => await _dbSet.FirstOrDefaultAsync(p => p.TransactionId == transactionId);

        public async Task<Payment?> GetByOrderIdAsync(string orderId)
            => await _dbSet.FirstOrDefaultAsync(p => p.OrderId == orderId);

        public async Task<IReadOnlyList<Payment>> GetPaymentsBySubscriptionAsync(Guid subscriptionId)
            => await _dbSet.Where(p => p.SubscriptionId == subscriptionId).ToListAsync();

        public async Task<IReadOnlyList<Payment>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to)
            => await _dbSet.Where(p => p.CreatedAt >= from && p.CreatedAt <= to).ToListAsync();

        public async Task<decimal> GetTotalCollectedAsync(DateTime from, DateTime to)
            => await _dbSet.Where(p => p.CreatedAt >= from && p.CreatedAt <= to && p.Status == PaymentStatus.Completed)
                .SumAsync(p => p.Amount.Amount);
    }
}
