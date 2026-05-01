using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.DTOs.Customer
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string VendorName { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public string? TrackingUrl { get; set; }
        public string? DeliveryAgentName { get; set; }
        public string? DeliveryAgentPhone { get; set; }
        public LocationDto? DeliveryAgentLocation { get; set; }
        public DateTime? EstimatedDeliveryTime { get; set; }
    }

    public class OrderItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class LocationDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
