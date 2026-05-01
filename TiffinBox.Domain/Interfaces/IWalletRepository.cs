using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Entities;

namespace TiffinBox.Domain.Interfaces
{
    public interface IWalletRepository : IRepository<Wallet>
    {
        Task<Wallet?> GetByUserIdAsync(int userId);
        Task<Wallet?> GetByUserIdWithTransactionsAsync(int userId, int? limit = null);
        Task<IReadOnlyList<WalletTransaction>> GetTransactionsAsync(int walletId, DateTime? from, DateTime? to, int page, int pageSize);
    }
}
