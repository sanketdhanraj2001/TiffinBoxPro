using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Entities;
using TiffinBox.Domain.Specifications;

namespace TiffinBox.Domain.Interfaces
{
    public interface IVendorRepository : IRepository<Vendor>
    {
        Task<Vendor?> GetByIdWithDetailsAsync(Guid id);
        Task<IReadOnlyList<Vendor>> GetApprovedVendorsAsync();
        Task<IReadOnlyList<Vendor>> GetPendingVendorsAsync();
        Task<IReadOnlyList<Vendor>> GetVendorsByAreaAsync(string area);
        Task<bool> IsGSTINUniqueAsync(string gstin, Guid? excludeVendorId = null);
        Task UpdateRatingAsync(Guid vendorId, int newRating);

        // ✅ Missing methods - Add these
        Task<IReadOnlyList<Vendor>> GetFilteredAsync(VendorSpecification spec);
        Task<int> CountFilteredAsync(VendorSpecification spec);
    }
}
