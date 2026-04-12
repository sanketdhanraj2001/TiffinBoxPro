using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Entities;
using TiffinBox.Domain.Enums;
using TiffinBox.Domain.Interfaces;
using TiffinBox.Domain.Specifications;

namespace TiffinBox.Infrastructure.Persistence.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IReadOnlyList<Order>> GetOrdersByCustomerAsync(Guid customerId, int page, int pageSize)
            => await _dbSet
                .Include(o => o.Subscription).ThenInclude(s => s.Plan).ThenInclude(p => p.Vendor)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .Where(o => o.Subscription.CustomerId == customerId)
                .OrderByDescending(o => o.DeliveryDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        public async Task<IReadOnlyList<Order>> GetOrdersByVendorAsync(Guid vendorId, OrderStatus? status, DateTime? fromDate, DateTime? toDate)
        {
            var query = _dbSet.Include(o => o.OrderItems).Where(o => o.Subscription.VendorId == vendorId);
            if (status.HasValue) query = query.Where(o => o.Status == status.Value);
            if (fromDate.HasValue) query = query.Where(o => o.DeliveryDate >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(o => o.DeliveryDate <= toDate.Value);
            return await query.OrderByDescending(o => o.DeliveryDate).ToListAsync();
        }

        public async Task<IReadOnlyList<Order>> GetOrdersByDeliveryAgentAsync(Guid agentId, OrderStatus? status)
        {
            var query = _dbSet.Where(o => o.DeliveryAgentId == agentId);
            if (status.HasValue) query = query.Where(o => o.Status == status.Value);
            return await query.OrderBy(o => o.DeliveryDate).ToListAsync();
        }

        public async Task<Order?> GetOrderWithTrackingAsync(Guid orderId)
            => await _dbSet
                .Include(o => o.Subscription).ThenInclude(s => s.Plan).ThenInclude(p => p.Vendor)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .Include(o => o.DeliveryAgent).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

        public async Task<IReadOnlyList<Order>> GetPendingOrdersForDateAsync(DateTime date)
            => await _dbSet
                .Include(o => o.Subscription)
                .Where(o => o.DeliveryDate == date && o.Status == OrderStatus.Pending)
                .ToListAsync();

        public async Task<bool> HasCustomerOrderedFromVendorAsync(Guid customerId, Guid vendorId)
            => await _dbSet
                .AnyAsync(o => o.Subscription.CustomerId == customerId
                    && o.Subscription.VendorId == vendorId
                    && o.Status == OrderStatus.Delivered);

        public async Task<decimal> GetVendorRevenueAsync(Guid vendorId, DateTime fromDate, DateTime toDate)
            => await _dbSet
                .Where(o => o.Subscription.VendorId == vendorId
                    && o.DeliveryDate >= fromDate
                    && o.DeliveryDate <= toDate
                    && o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount.Amount);

        public async Task<IReadOnlyList<Order>> GetFilteredAsync(OrderSpecification spec)
        {
            var query = _dbSet.AsQueryable();
            if (spec.CustomerId.HasValue) query = query.Where(o => o.Subscription.CustomerId == spec.CustomerId.Value);
            if (spec.VendorId.HasValue) query = query.Where(o => o.Subscription.VendorId == spec.VendorId.Value);
            if (spec.AgentId.HasValue) query = query.Where(o => o.DeliveryAgentId == spec.AgentId.Value);
            if (spec.Status.HasValue) query = query.Where(o => o.Status == spec.Status.Value);
            if (spec.FromDate.HasValue) query = query.Where(o => o.DeliveryDate >= spec.FromDate.Value);
            if (spec.ToDate.HasValue) query = query.Where(o => o.DeliveryDate <= spec.ToDate.Value);
            if (spec.IncludeItems) query = query.Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem);
            query = query.Include(o => o.Subscription).ThenInclude(s => s.Plan).ThenInclude(p => p.Vendor);
            return await query.OrderByDescending(o => o.DeliveryDate)
                .Skip((spec.Page - 1) * spec.PageSize).Take(spec.PageSize).ToListAsync();
        }

        public async Task<int> CountFilteredAsync(OrderSpecification spec)
        {
            var query = _dbSet.AsQueryable();
            if (spec.CustomerId.HasValue) query = query.Where(o => o.Subscription.CustomerId == spec.CustomerId.Value);
            if (spec.VendorId.HasValue) query = query.Where(o => o.Subscription.VendorId == spec.VendorId.Value);
            if (spec.AgentId.HasValue) query = query.Where(o => o.DeliveryAgentId == spec.AgentId.Value);
            if (spec.Status.HasValue) query = query.Where(o => o.Status == spec.Status.Value);
            if (spec.FromDate.HasValue) query = query.Where(o => o.DeliveryDate >= spec.FromDate.Value);
            if (spec.ToDate.HasValue) query = query.Where(o => o.DeliveryDate <= spec.ToDate.Value);
            return await query.CountAsync();
        }
    }
}
