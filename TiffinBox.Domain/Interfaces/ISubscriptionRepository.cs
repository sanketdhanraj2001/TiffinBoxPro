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
        Task<Subscription?> GetByIdWithDetailsAsync(Guid id);
        Task<IReadOnlyList<Subscription>> GetCustomerSubscriptionsAsync(Guid customerId);
        Task<IReadOnlyList<Subscription>> GetVendorSubscriptionsAsync(Guid vendorId);
        Task<Subscription?> GetActiveSubscriptionAsync(Guid customerId, Guid vendorId);
        Task<IReadOnlyList<Subscription>> GetExpiringSubscriptionsAsync(DateTime cutoffDate);
        Task<IReadOnlyList<Subscription>> GetSubscriptionsForRenewalAsync(DateTime renewalDate);
        Task<int> GetActiveSubscribersCountAsync(Guid vendorId);
    }
}
