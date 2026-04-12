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
        Task<IReadOnlyList<SubscriptionPlan>> GetActivePlansByVendorAsync(Guid vendorId);
        Task<SubscriptionPlan?> GetByIdWithDetailsAsync(Guid id);
        Task<bool> IsPlanNameUniqueAsync(Guid vendorId, string name, Guid? excludeId = null);
    }

    public interface IMenuItemRepository : IRepository<MenuItem>
    {
        Task<IReadOnlyList<MenuItem>> GetByVendorAsync(Guid vendorId);
        Task<IReadOnlyList<MenuItem>> GetByVendorAsync(Guid vendorId, bool onlyAvailable);
        Task<IReadOnlyList<MenuItem>> GetByIdsAsync(List<Guid> ids);  
        Task<IReadOnlyList<MenuItem>> GetByCategoryAsync(Guid vendorId, string category);
        Task<IReadOnlyList<MenuItem>> GetVegetarianItemsAsync(Guid vendorId);
        Task<IReadOnlyList<MenuItem>> GetPopularItemsAsync(Guid vendorId, int take);
        Task<bool> IsNameUniqueAsync(Guid vendorId, string name, Guid? excludeId = null);
    }


    public interface IReviewRepository : IRepository<Review>
    {
        Task<IReadOnlyList<Review>> GetByVendorAsync(Guid vendorId, int page, int pageSize);
        Task<IReadOnlyList<Review>> GetByCustomerAsync(Guid customerId, int page, int pageSize);
        Task<Review?> GetCustomerReviewForVendorAsync(Guid customerId, Guid vendorId);  
        Task<IReadOnlyList<Review>> GetPendingApprovalAsync();
        Task<double> GetAverageRatingForVendorAsync(Guid vendorId);
        Task<int> GetTotalReviewsForVendorAsync(Guid vendorId);
        Task<IReadOnlyList<Review>> GetRecentReviewsAsync(Guid vendorId, int take);
        Task<bool> HasCustomerReviewedVendorAsync(Guid customerId, Guid vendorId);
    }
}
