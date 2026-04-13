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
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.Comment)
                .HasMaxLength(1000);

            builder.Property(r => r.Reply)
                .HasMaxLength(1000);

            builder.Property(r => r.Images)
                .HasConversion(
                    v => string.Join(',', v ?? new List<string>()),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

            // ✅ Relationship with Customer - Use NoAction
            builder.HasOne(r => r.Customer)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);  // Changed from Cascade

            // ✅ Relationship with Vendor - Use NoAction
            builder.HasOne(r => r.Vendor)
                .WithMany(v => v.Reviews)
                .HasForeignKey(r => r.VendorId)
                .OnDelete(DeleteBehavior.NoAction);  // Changed from Cascade

            // ✅ Relationship with Order - Use NoAction (optional)
            builder.HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes
            builder.HasIndex(r => r.CustomerId);
            builder.HasIndex(r => r.VendorId);
            builder.HasIndex(r => r.OrderId);
            builder.HasIndex(r => r.Rating);
            builder.HasIndex(r => r.CreatedAt);
            builder.HasIndex(r => r.IsApproved);
        }
    }
}
