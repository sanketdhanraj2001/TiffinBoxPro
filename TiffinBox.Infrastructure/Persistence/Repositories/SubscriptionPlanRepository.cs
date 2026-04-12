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
    public class SubscriptionPlanRepository : GenericRepository<SubscriptionPlan>, ISubscriptionPlanRepository
    {
        public SubscriptionPlanRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IReadOnlyList<SubscriptionPlan>> GetActivePlansByVendorAsync(Guid vendorId)
        {
            return await _dbSet
                .Include(p => p.Vendor)
                .Where(p => p.VendorId == vendorId && p.IsActive)
                .OrderBy(p => p.Price.Amount)
                .ToListAsync();
        }

        public async Task<SubscriptionPlan?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(p => p.Vendor)
                .Include(p => p.Subscriptions)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> IsPlanNameUniqueAsync(Guid vendorId, string name, Guid? excludeId = null)
        {
            var query = _dbSet.Where(p => p.VendorId == vendorId && p.Name == name);
            if (excludeId.HasValue)
                query = query.Where(p => p.Id != excludeId.Value);
            return !await query.AnyAsync();
        }
    }
}
