using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Enums;

namespace TiffinBox.Domain.Specifications
{
    public class OrderSpecification
    {
        public Guid? CustomerId { get; set; }
        public Guid? VendorId { get; set; }
        public Guid? AgentId { get; set; }
        public OrderStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IncludeItems { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
