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
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("Wallets");

            builder.HasKey(w => w.Id);

            // Removed .HasDefaultValue(0) - Money type cannot have integer default
            builder.Property(w => w.Balance)
                .HasConversion(
                    v => v.Amount,
                    v => new Money(v, "INR"))
                .HasPrecision(18, 2)
                .IsRequired();  // Required instead of HasDefaultValue

            builder.Property(w => w.IsActive)
                .HasDefaultValue(true);

            // Fix: Use Restrict for User relationship
            builder.HasOne(w => w.User)
                .WithOne(u => u.Wallet)
                .HasForeignKey<Wallet>(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(w => w.UserId).IsUnique();
        }
    }
}
