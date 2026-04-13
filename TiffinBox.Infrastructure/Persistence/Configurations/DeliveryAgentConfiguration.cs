using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Entities;

namespace TiffinBox.Infrastructure.Persistence.Configurations
{
    public class DeliveryAgentConfiguration : IEntityTypeConfiguration<DeliveryAgent>
    {
        public void Configure(EntityTypeBuilder<DeliveryAgent> builder)
        {
            builder.ToTable("DeliveryAgents");

            builder.HasKey(da => da.Id);

            builder.Property(da => da.VehicleNumber)
                .HasMaxLength(50);

            builder.Property(da => da.VehicleType)
                .HasMaxLength(50);

            builder.Property(da => da.Rating)
                .HasDefaultValue(0);

            builder.Property(da => da.TotalRatings)
                .HasDefaultValue(0);

            builder.Property(da => da.TotalDeliveries)
                .HasDefaultValue(0);

            // ✅ FIX: Use Restrict instead of Cascade to avoid multiple cascade paths
            builder.HasOne(da => da.Vendor)
                .WithMany(v => v.DeliveryAgents)
                .HasForeignKey(da => da.VendorId)
                .OnDelete(DeleteBehavior.Restrict);  // Changed from Cascade to Restrict

            builder.HasOne(da => da.User)
                .WithOne(u => u.DeliveryAgent)
                .HasForeignKey<DeliveryAgent>(da => da.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(da => da.UserId).IsUnique();
            builder.HasIndex(da => da.VendorId);
            builder.HasIndex(da => da.IsAvailable);
            builder.HasIndex(da => da.IsActive);
        }
    }
}
