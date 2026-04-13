using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Entities;
using TiffinBox.Domain.ValueObjects;

namespace TiffinBox.Infrastructure.Persistence.Configurations
{
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.ToTable("Subscriptions");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.StartDate)
                .IsRequired();

            builder.Property(s => s.EndDate)
                .IsRequired();

            builder.Property(s => s.Status)
                .HasConversion<int>();

            builder.Property(s => s.TotalAmount)
                .HasConversion(
                    v => v.Amount,
                    v => new Money(v, "INR"))
                .HasPrecision(18, 2);

            builder.Property(s => s.TotalDays)
                .HasDefaultValue(0);

            builder.Property(s => s.DeliveredDays)
                .HasDefaultValue(0);

            builder.Property(s => s.SkippedDays)
                .HasDefaultValue(0);

            builder.Property(s => s.CancelledAt)
                .IsRequired(false);

            builder.Property(s => s.CancellationReason)
                .HasMaxLength(500)
                .IsRequired(false);

            // ✅ Relationship with SubscriptionPlan
            builder.HasOne(s => s.Plan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.PlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Relationship with Customer - Use NoAction to avoid cascade paths
            builder.HasOne(s => s.Customer)
                .WithMany()
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);  // Changed from Restrict to NoAction

            // ✅ Relationship with Vendor - Use NoAction to avoid cascade paths
            builder.HasOne(s => s.Vendor)
                .WithMany()
                .HasForeignKey(s => s.VendorId)
                .OnDelete(DeleteBehavior.NoAction);  // Changed from Restrict to NoAction

            // Indexes for better performance
            builder.HasIndex(s => s.CustomerId);
            builder.HasIndex(s => s.VendorId);
            builder.HasIndex(s => s.PlanId);
            builder.HasIndex(s => s.Status);
            builder.HasIndex(s => s.StartDate);
            builder.HasIndex(s => s.EndDate);
            builder.HasIndex(s => new { s.CustomerId, s.VendorId, s.Status });
        }
    }
    public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
        {
            builder.ToTable("SubscriptionPlans");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Description)
                .HasMaxLength(500);

            builder.Property(p => p.PlanType)
                .HasConversion<int>();

            builder.Property(p => p.Price)
                .HasConversion(
                    v => v.Amount,
                    v => new Money(v, "INR"))
                .HasPrecision(18, 2);

            builder.Property(p => p.DurationDays)
                .IsRequired();

            builder.Property(p => p.MealsPerDay)
                .IsRequired();

            builder.Property(p => p.IsActive)
                .HasDefaultValue(true);

            builder.Property(p => p.MaxSubscribers)
                .HasDefaultValue(50);

            builder.Property(p => p.CurrentSubscribers)
                .HasDefaultValue(0);

            // ✅ Use NoAction for Vendor relationship
            builder.HasOne(p => p.Vendor)
                .WithMany(v => v.SubscriptionPlans)
                .HasForeignKey(p => p.VendorId)
                .OnDelete(DeleteBehavior.NoAction);  // Changed from Restrict to NoAction

            builder.HasIndex(p => p.VendorId);
            builder.HasIndex(p => p.IsActive);
            builder.HasIndex(p => p.PlanType);
        }
    }
}
