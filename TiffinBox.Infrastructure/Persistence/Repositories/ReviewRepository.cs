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
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IReadOnlyList<Review>> GetByVendorAsync(int vendorId, int page, int pageSize)
        {
            return await _dbSet
                .Include(r => r.Customer)
                .Where(r => r.VendorId == vendorId && r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Review>> GetByCustomerAsync(int customerId, int page, int pageSize)
        {
            return await _dbSet
                .Include(r => r.Vendor)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Review?> GetCustomerReviewForVendorAsync(int customerId, int vendorId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.CustomerId == customerId && r.VendorId == vendorId);
        }

        public async Task<IReadOnlyList<Review>> GetPendingApprovalAsync()
        {
            return await _dbSet
                .Include(r => r.Customer)
                .Include(r => r.Vendor)
                .Where(r => !r.IsApproved)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingForVendorAsync(int vendorId)
        {
            var avg = await _dbSet
                .Where(r => r.VendorId == vendorId && r.IsApproved)
                .AverageAsync(r => (double?)r.Rating);

            return avg ?? 0;
        }

        public async Task<int> GetTotalReviewsForVendorAsync(int vendorId)
        {
            return await _dbSet
                .CountAsync(r => r.VendorId == vendorId && r.IsApproved);
        }

        public async Task<IReadOnlyList<Review>> GetRecentReviewsAsync(int vendorId, int take)
        {
            return await _dbSet
                .Include(r => r.Customer)
                .Where(r => r.VendorId == vendorId && r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<bool> HasCustomerReviewedVendorAsync(int customerId, int vendorId)
        {
            return await _dbSet
                .AnyAsync(r => r.CustomerId == customerId && r.VendorId == vendorId);
        }
    }
}
