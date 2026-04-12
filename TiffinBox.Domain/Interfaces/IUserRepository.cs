using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Entities;

namespace TiffinBox.Domain.Interfaces
{
    public interface IUserRepository:IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneAsync(string phoneNumber);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task<User?> GetByIdWithDetailsAsync(Guid id);
        Task<IReadOnlyList<User>> GetUsersByRoleAsync(string role, int page, int pageSize);
        Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null);
    }
}
