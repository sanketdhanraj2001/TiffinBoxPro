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
    public class SubscriptionRepository : GenericRepository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Subscription?> GetByIdWithDetailsAsync(int id)
            => await _dbSet
                .Include(s => s.Plan).ThenInclude(p => p.Vendor)
                .Include(s => s.Meals).ThenInclude(m => m.MenuItem)
                .Include(s => s.Orders)
                .FirstOrDefaultAsync(s => s.Id == id);

        public async Task<IReadOnlyList<Subscription>> GetCustomerSubscriptionsAsync(int customerId)
            => await _dbSet
                .Include(s => s.Plan).ThenInclude(p => p.Vendor)
                .Where(s => s.CustomerId == customerId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

        public async Task<IReadOnlyList<Subscription>> GetVendorSubscriptionsAsync(int vendorId)
            => await _dbSet
                .Include(s => s.Plan)
                .Where(s => s.VendorId == vendorId)
                .ToListAsync();

        public async Task<Subscription?> GetActiveSubscriptionAsync(int customerId, int vendorId)
            => await _dbSet
                .FirstOrDefaultAsync(s => s.CustomerId == customerId
                    && s.VendorId == vendorId
                    && s.Status == SubscriptionStatus.Active);

        public async Task<IReadOnlyList<Subscription>> GetExpiringSubscriptionsAsync(DateTime cutoffDate)
            => await _dbSet
                .Where(s => s.EndDate <= cutoffDate && s.Status == SubscriptionStatus.Active)
                .ToListAsync();

        public async Task<IReadOnlyList<Subscription>> GetSubscriptionsForRenewalAsync(DateTime renewalDate)
            => await _dbSet
                .Where(s => s.EndDate.Date == renewalDate.Date && s.Status == SubscriptionStatus.Active)
                .ToListAsync();

        public async Task<int> GetActiveSubscribersCountAsync(int vendorId)
            => await _dbSet.CountAsync(s => s.VendorId == vendorId && s.Status == SubscriptionStatus.Active);
    }
}
