using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.DTOs.Customer
{
    public class VendorListDto
    {
        public int Id { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? Description { get; set; }
        public double Rating { get; set; }
        public int TotalRatings { get; set; }
        public string? Cuisine { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int EstimatedDeliveryTime { get; set; }
        public bool IsOpen { get; set; }
        public string? Distance { get; set; }
    }
}
