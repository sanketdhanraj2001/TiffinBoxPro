using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Entities;

namespace TiffinBox.Domain.Specifications
{
    public class VendorSpecification
    {
        public bool IsApproved { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public string? Cuisine { get; set; }
        public string? Area { get; set; }
        public int? MinRating { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }

        public bool IncludeMenuItems { get; set; } = true;
        public bool IncludeSubscriptionPlans { get; set; } = true;
        public bool IncludeReviews { get; set; } = true;
        public bool IncludeUser { get; set; } = true;

        public IQueryable<Vendor> ApplyFiltersOnly(IQueryable<Vendor> query)
        {
            query = query.Where(v => v.IsApproved == IsApproved);
            query = query.Where(v => v.IsActive == IsActive);

            if (!string.IsNullOrWhiteSpace(Cuisine))
                query = query.Where(v => v.Cuisine != null && v.Cuisine.Contains(Cuisine));

            if (!string.IsNullOrWhiteSpace(Area))
                query = query.Where(v =>
                    v.BusinessAddress.City.Contains(Area) ||
                    v.ServiceAreas.Any(a => a.Contains(Area)));

            if (MinRating.HasValue)
                query = query.Where(v => v.Rating >= MinRating.Value);

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(v =>
                    v.BusinessName.Contains(SearchTerm) ||
                    (v.Description != null && v.Description.Contains(SearchTerm)) ||
                    (v.Cuisine != null && v.Cuisine.Contains(SearchTerm)));
            }

            return query;
        }

        public IQueryable<Vendor> ApplyFilters(IQueryable<Vendor> query)
        {
            return ApplyFiltersOnly(query);
        }

        public IQueryable<Vendor> ApplyIncludes(IQueryable<Vendor> query)
        {
            if (IncludeMenuItems)
                query = query.Include(v => v.MenuItems);

            if (IncludeSubscriptionPlans)
                query = query.Include(v => v.SubscriptionPlans);

            if (IncludeReviews)
                query = query.Include(v => v.Reviews)
                             .ThenInclude(r => r.Customer);  // ✅ Include customer for review username

            if (IncludeUser)
                query = query.Include(v => v.User);

            return query;
        }

        public IQueryable<Vendor> ApplySorting(IQueryable<Vendor> query)
        {
            return query.OrderByDescending(v => v.Rating);
        }

        public IQueryable<Vendor> ApplyPagination(IQueryable<Vendor> query)
        {
            return query.Skip((Page - 1) * PageSize).Take(PageSize);
        }

        public IQueryable<Vendor> Apply(IQueryable<Vendor> query)
        {
            query = ApplyFilters(query);
            query = ApplyIncludes(query);
            query = ApplySorting(query);
            query = ApplyPagination(query);
            return query;
        }

        public async Task<int> GetTotalCountAsync(IQueryable<Vendor> query)
        {
            var filteredQuery = ApplyFiltersOnly(query);
            return await filteredQuery.CountAsync();
        }
    }
}