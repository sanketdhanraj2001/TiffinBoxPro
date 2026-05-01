using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Entities;

namespace TiffinBox.Domain.Interfaces
{
    public interface ISubscriptionRepository : IRepository<Subscription>
    {
        Task<Subscription?> GetByIdWithDetailsAsync(int id);
        Task<IReadOnlyList<Subscription>> GetCustomerSubscriptionsAsync(int customerId);
        Task<IReadOnlyList<Subscription>> GetVendorSubscriptionsAsync(int vendorId);
        Task<Subscription?> GetActiveSubscriptionAsync(int customerId, int vendorId);
        Task<IReadOnlyList<Subscription>> GetExpiringSubscriptionsAsync(DateTime cutoffDate);
        Task<IReadOnlyList<Subscription>> GetSubscriptionsForRenewalAsync(DateTime renewalDate);
        Task<int> GetActiveSubscribersCountAsync(int vendorId);
    }
}
