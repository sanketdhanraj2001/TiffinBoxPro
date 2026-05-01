using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Common;
using TiffinBox.Domain.ValueObjects;

namespace TiffinBox.Domain.Entities
{
    public class MenuItem : BaseEntity
    {
        public int VendorId { get; private set; }
        public virtual Vendor Vendor { get; private set; }
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public Money Price { get; private set; }
        public string Category { get; private set; }
        public string? ImageUrl { get; private set; }
        public bool IsAvailable { get; private set; }
        public bool IsVegetarian { get; private set; }
        public bool IsVegan { get; private set; }
        public bool IsGlutenFree { get; private set; }
        public int PreparationTime { get; private set; } = 15; // in minutes
        public int Calories { get; private set; }
        public int? StockQuantity { get; private set; }
        public int OrderCount { get; private set; }
        public double Rating { get; private set; }
        public int TotalRatings { get; private set; }

        private MenuItem() { }

        public static MenuItem Create(
            int vendorId,
            string name,
            Money price,
            string category,
            string? description = null,
            string? imageUrl = null,
            bool isVegetarian = false,
            bool isVegan = false,
            bool isGlutenFree = false,
            int preparationTime = 15,
            int calories = 0,
            int? stockQuantity = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Menu item name is required");

            if (price.Amount <= 0)
                throw new ArgumentException("Price must be greater than zero");

            return new MenuItem
            {
                VendorId = vendorId,
                Name = name,
                Description = description,
                Price = price,
                Category = category,
                ImageUrl = imageUrl,
                IsAvailable = true,
                IsVegetarian = isVegetarian,
                IsVegan = isVegan,
                IsGlutenFree = isGlutenFree,
                PreparationTime = preparationTime,
                Calories = calories,
                StockQuantity = stockQuantity,
                Rating = 0,
                TotalRatings = 0,
                OrderCount = 0
            };
        }

        public void UpdateDetails(
            string? name,
            string? description,
            Money? price,
            string? category,
            string? imageUrl,
            int? preparationTime,
            int? calories,
            int? stockQuantity)
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;

            if (description != null)
                Description = description;

            if (price != null && price.Amount > 0)
                Price = price;

            if (!string.IsNullOrWhiteSpace(category))
                Category = category;

            if (imageUrl != null)
                ImageUrl = imageUrl;

            if (preparationTime.HasValue && preparationTime.Value > 0)
                PreparationTime = preparationTime.Value;

            if (calories.HasValue)
                Calories = calories.Value;

            if (stockQuantity.HasValue)
                StockQuantity = stockQuantity.Value;

            UpdateTimestamp();
        }

        public void SetAvailability(bool isAvailable)
        {
            IsAvailable = isAvailable;
            UpdateTimestamp();
        }

        public void SetVegetarian(bool isVegetarian)
        {
            IsVegetarian = isVegetarian;
            UpdateTimestamp();
        }

        public void SetVegan(bool isVegan)
        {
            IsVegan = isVegan;
            UpdateTimestamp();
        }

        public void SetGlutenFree(bool isGlutenFree)
        {
            IsGlutenFree = isGlutenFree;
            UpdateTimestamp();
        }

        public void UpdatePrice(Money newPrice)
        {
            if (newPrice.Amount <= 0)
                throw new ArgumentException("Price must be greater than zero");

            Price = newPrice;
            UpdateTimestamp();
        }

        public void ReduceStock(int quantity)
        {
            if (!StockQuantity.HasValue)
                return;

            if (StockQuantity.Value < quantity)
                throw new InvalidOperationException("Insufficient stock");

            StockQuantity -= quantity;
            UpdateTimestamp();
        }

        public void IncreaseStock(int quantity)
        {
            if (!StockQuantity.HasValue)
                return;

            StockQuantity += quantity;
            UpdateTimestamp();
        }

        public void IncrementOrderCount()
        {
            OrderCount++;
            UpdateTimestamp();
        }

        public void AddRating(int rating)
        {
            var total = (Rating * TotalRatings) + rating;
            TotalRatings++;
            Rating = total / TotalRatings;
            UpdateTimestamp();
        }

        public bool IsInStock()
        {
            return !StockQuantity.HasValue || StockQuantity.Value > 0;
        }

        public bool IsOrderable()
        {
            return IsAvailable && IsInStock();
        }
    }
}
