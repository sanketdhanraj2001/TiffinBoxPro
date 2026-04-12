using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.DTOs.Customer
{
    public class SubscriptionDto
    {
        public Guid Id { get; set; }
        public Guid VendorId { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? PaymentOrderId { get; set; }
        public int DeliveredDays { get; set; }
        public int TotalDays { get; set; }
        public int SkippedDays { get; set; }
        public bool CanCancel { get; set; }
        public bool CanPause { get; set; }
    }
}
