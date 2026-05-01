using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Entities;

namespace TiffinBox.Domain.Interfaces
{
    public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
    {
        Task<IReadOnlyList<SubscriptionPlan>> GetActivePlansByVendorAsync(int vendorId);
        Task<SubscriptionPlan?> GetByIdWithDetailsAsync(int id);
        Task<bool> IsPlanNameUniqueAsync(int vendorId, string name, int? excludeId = null);
    }

    public interface IMenuItemRepository : IRepository<MenuItem>
    {
        Task<IReadOnlyList<MenuItem>> GetByVendorAsync(int vendorId);
        Task<IReadOnlyList<MenuItem>> GetByVendorAsync(int vendorId, bool onlyAvailable);
        Task<IReadOnlyList<MenuItem>> GetByIdsAsync(List<int> ids);  
        Task<IReadOnlyList<MenuItem>> GetByCategoryAsync(int vendorId, string category);
        Task<IReadOnlyList<MenuItem>> GetVegetarianItemsAsync(int vendorId);
        Task<IReadOnlyList<MenuItem>> GetPopularItemsAsync(int vendorId, int take);
        Task<bool> IsNameUniqueAsync(int vendorId, string name, int? excludeId = null);
    }


    public interface IReviewRepository : IRepository<Review>
    {
        Task<IReadOnlyList<Review>> GetByVendorAsync(int vendorId, int page, int pageSize);
        Task<IReadOnlyList<Review>> GetByCustomerAsync(int customerId, int page, int pageSize);
        Task<Review?> GetCustomerReviewForVendorAsync(int customerId, int vendorId);  
        Task<IReadOnlyList<Review>> GetPendingApprovalAsync();
        Task<double> GetAverageRatingForVendorAsync(int vendorId);
        Task<int> GetTotalReviewsForVendorAsync(int vendorId);
        Task<IReadOnlyList<Review>> GetRecentReviewsAsync(int vendorId, int take);
        Task<bool> HasCustomerReviewedVendorAsync(int customerId, int vendorId);
    }
}
