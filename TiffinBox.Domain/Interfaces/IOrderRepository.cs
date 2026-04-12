using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Entities;
using TiffinBox.Domain.Enums;
using TiffinBox.Domain.Specifications;

namespace TiffinBox.Domain.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IReadOnlyList<Order>> GetOrdersByCustomerAsync(Guid customerId, int page, int pageSize);
        Task<IReadOnlyList<Order>> GetOrdersByVendorAsync(Guid vendorId, OrderStatus? status, DateTime? fromDate, DateTime? toDate);
        Task<IReadOnlyList<Order>> GetOrdersByDeliveryAgentAsync(Guid agentId, OrderStatus? status);
        Task<Order?> GetOrderWithTrackingAsync(Guid orderId);
        Task<IReadOnlyList<Order>> GetPendingOrdersForDateAsync(DateTime date);
        Task<bool> HasCustomerOrderedFromVendorAsync(Guid customerId, Guid vendorId);
        Task<decimal> GetVendorRevenueAsync(Guid vendorId, DateTime fromDate, DateTime toDate);
        Task<IReadOnlyList<Order>> GetFilteredAsync(OrderSpecification spec);
        Task<int> CountFilteredAsync(OrderSpecification spec);

    }
}
