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
    public class MenuItemRepository : GenericRepository<MenuItem>, IMenuItemRepository
    {
        public MenuItemRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IReadOnlyList<MenuItem>> GetByVendorAsync(int vendorId)
        {
            return await _dbSet
                .Where(m => m.VendorId == vendorId)
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<MenuItem>> GetByVendorAsync(int vendorId, bool onlyAvailable)
        {
            var query = _dbSet.Where(m => m.VendorId == vendorId);

            if (onlyAvailable)
                query = query.Where(m => m.IsAvailable);

            return await query
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<MenuItem>> GetByIdsAsync(List<int> ids)
        {
            if (ids == null || !ids.Any())
                return new List<MenuItem>();

            return await _dbSet
                .Where(m => ids.Contains(m.Id))
                .ToListAsync();
        }

        public async Task<IReadOnlyList<MenuItem>> GetByCategoryAsync(int vendorId, string category)
        {
            return await _dbSet
                .Where(m => m.VendorId == vendorId && m.Category == category && m.IsAvailable)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<MenuItem>> GetVegetarianItemsAsync(int vendorId)
        {
            return await _dbSet
                .Where(m => m.VendorId == vendorId && m.IsVegetarian && m.IsAvailable)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<MenuItem>> GetPopularItemsAsync(int vendorId, int take)
        {
            return await _dbSet
                .Where(m => m.VendorId == vendorId && m.IsAvailable)
                .OrderByDescending(m => m.OrderCount)
                .Take(take)
                .ToListAsync();
        }

        public async Task<bool> IsNameUniqueAsync(int vendorId, string name, int? excludeId = null)
        {
            var query = _dbSet.Where(m => m.VendorId == vendorId && m.Name == name);
            if (excludeId.HasValue)
                query = query.Where(m => m.Id != excludeId.Value);
            return !await query.AnyAsync();
        }
    }
}
