using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.DTOs.Customer
{
    public class VendorDetailDto
    {
        public Guid Id { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public double Rating { get; set; }
        public int TotalRatings { get; set; }
        public string? Cuisine { get; set; }
        public string Address { get; set; } = string.Empty;
        public TimeOnly OpeningTime { get; set; }
        public TimeOnly ClosingTime { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int EstimatedDeliveryTime { get; set; }
        public List<MenuItemDto> MenuItems { get; set; } = new();
        public List<PlanDto> Plans { get; set; } = new();
        public List<ReviewDto> Reviews { get; set; } = new();
    }

    public class MenuItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsVegetarian { get; set; }
    }

    public class PlanDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string PlanType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public int MealsPerDay { get; set; }
    }

    public class ReviewDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
