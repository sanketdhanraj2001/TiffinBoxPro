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
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Include(u => u.Vendor)
                .Include(u => u.DeliveryAgent)
                .Include(u => u.Wallet)
                .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant() || u.UserName == email);
        }

        public async Task<User?> GetByPhoneAsync(string phoneNumber)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _dbSet
                .Include(u => u.Vendor)
                .Include(u => u.DeliveryAgent)
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        public async Task<User?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(u => u.Vendor)
                .Include(u => u.DeliveryAgent)
                .Include(u => u.Wallet)
                .ThenInclude(w => w.Transactions)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IReadOnlyList<User>> GetUsersByRoleAsync(string role, int page, int pageSize)
        {
            return await _dbSet
                .Where(u => u.Role.ToString() == role && u.IsActive)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null)
        {
            var query = _dbSet.Where(u => u.Email == email.ToLowerInvariant());

            if (excludeUserId.HasValue)
                query = query.Where(u => u.Id != excludeUserId.Value);

            return !await query.AnyAsync();
        }
    }
}
