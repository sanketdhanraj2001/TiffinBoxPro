using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Entities;
using TiffinBox.Domain.Interfaces;
using TiffinBox.Domain.Specifications;

namespace TiffinBox.Infrastructure.Persistence.Repositories
{
    public class VendorRepository : GenericRepository<Vendor>, IVendorRepository
    {
        public VendorRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Vendor?> GetByIdWithDetailsAsync(int id)
            => await _dbSet
                .Include(v => v.User)
                .Include(v => v.MenuItems)
                .Include(v => v.SubscriptionPlans)
                .Include(v => v.Reviews)
                .FirstOrDefaultAsync(v => v.Id == id);

        public async Task<IReadOnlyList<Vendor>> GetApprovedVendorsAsync()
            => await _dbSet
                .Where(v => v.IsApproved && v.IsActive)
                .OrderByDescending(v => v.Rating)
                .ToListAsync();

        public async Task<IReadOnlyList<Vendor>> GetPendingVendorsAsync()
            => await _dbSet
                .Where(v => !v.IsApproved && v.IsActive)
                .ToListAsync();

        public async Task<IReadOnlyList<Vendor>> GetVendorsByAreaAsync(string area)
            => await _dbSet
                .Where(v => v.BusinessAddress.City.Contains(area) && v.IsApproved)
                .ToListAsync();

        public async Task<bool> IsGSTINUniqueAsync(string gstin, int? excludeVendorId = null)
        {
            var query = _dbSet.Where(v => v.GSTIN == gstin);
            if (excludeVendorId.HasValue)
                query = query.Where(v => v.Id != excludeVendorId.Value);
            return !await query.AnyAsync();
        }

        public async Task UpdateRatingAsync(int vendorId, int newRating)
        {
            var vendor = await GetByIdAsync(vendorId);
            if (vendor != null)
            {
                vendor.AddRating(newRating);
                await UpdateAsync(vendor);
            }
        }

        public async Task<IReadOnlyList<Vendor>> GetFilteredAsync(VendorSpecification spec)
        {
            var query = _dbSet.AsQueryable();

            if (spec.IsApproved)
                query = query.Where(v => v.IsApproved == spec.IsApproved);

            if (spec.IsActive)
                query = query.Where(v => v.IsActive == spec.IsActive);

            if (!string.IsNullOrWhiteSpace(spec.Cuisine))
                query = query.Where(v => v.Cuisine != null && v.Cuisine.Contains(spec.Cuisine));

            if (!string.IsNullOrWhiteSpace(spec.Area))
                query = query.Where(v => v.BusinessAddress.City.Contains(spec.Area) ||
                                         v.ServiceAreas.Any(a => a.Contains(spec.Area)));

            if (spec.MinRating.HasValue)
                query = query.Where(v => v.Rating >= spec.MinRating.Value);

            // Apply sorting
            query = query.OrderByDescending(v => v.Rating);

            // Apply pagination
            query = query
                .Skip((spec.Page - 1) * spec.PageSize)
                .Take(spec.PageSize);

            return await query.ToListAsync();
        }

        public async Task<int> CountFilteredAsync(VendorSpecification spec)
        {
            var query = _dbSet.AsQueryable();

            // Apply filters (same as above but without pagination)
            if (spec.IsApproved)
                query = query.Where(v => v.IsApproved == spec.IsApproved);

            if (spec.IsActive)
                query = query.Where(v => v.IsActive == spec.IsActive);

            if (!string.IsNullOrWhiteSpace(spec.Cuisine))
                query = query.Where(v => v.Cuisine != null && v.Cuisine.Contains(spec.Cuisine));

            if (!string.IsNullOrWhiteSpace(spec.Area))
                query = query.Where(v => v.BusinessAddress.City.Contains(spec.Area) ||
                                         v.ServiceAreas.Any(a => a.Contains(spec.Area)));

            if (spec.MinRating.HasValue)
                query = query.Where(v => v.Rating >= spec.MinRating.Value);

            return await query.CountAsync();
        }
    }
}
