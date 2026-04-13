using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Entities;
using TiffinBox.Domain.Interfaces;

namespace TiffinBox.Infrastructure.Persistence.Repositories
{
    public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
    {
        public WalletRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Wallet?> GetByUserIdAsync(Guid userId)
            => await _dbSet.FirstOrDefaultAsync(w => w.UserId == userId);

        public async Task<Wallet?> GetByUserIdWithTransactionsAsync(Guid userId, int? limit = null)
        {
            var query = _dbSet.Include(w => w.Transactions).Where(w => w.UserId == userId);
            if (limit.HasValue)
                query = query.Include(w => w.Transactions.OrderByDescending(t => t.CreatedAt).Take(limit.Value));
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<WalletTransaction>> GetTransactionsAsync(Guid walletId, DateTime? from, DateTime? to, int page, int pageSize)
        {
            var query = _context.Set<WalletTransaction>().Where(t => t.WalletId == walletId);
            if (from.HasValue) query = query.Where(t => t.CreatedAt >= from.Value);
            if (to.HasValue) query = query.Where(t => t.CreatedAt <= to.Value);
            return await query.OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<decimal> GetBalanceAsync(Guid userId)
        {
            var wallet = await GetByUserIdAsync(userId);
            return wallet?.Balance.Amount ?? 0;
        }

        public async Task<bool> CreditAsync(Guid walletId, decimal amount, string description, string? referenceId = null)
        {
            var wallet = await GetByIdAsync(walletId);
            if (wallet == null) return false;
            wallet.Credit(amount, description, referenceId);
            await UpdateAsync(wallet);
            return true;
        }

        public async Task<bool> DebitAsync(Guid walletId, decimal amount, string description, string? referenceId = null)
        {
            var wallet = await GetByIdAsync(walletId);
            if (wallet == null) return false;
            if (wallet.Balance.Amount < amount) return false;
            wallet.Debit(amount, description, referenceId);
            await UpdateAsync(wallet);
            return true;
        }
    }
}
