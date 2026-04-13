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
    public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.ToTable("WalletTransactions");

            builder.HasKey(wt => wt.Id);

            builder.Property(wt => wt.Type)
                .HasConversion<int>();

            builder.Property(wt => wt.Amount)
                .HasConversion(
                    v => v.Amount,
                    v => new Money(v, "INR"))
                .HasPrecision(18, 2);

            builder.Property(wt => wt.BalanceAfter)
                .HasPrecision(18, 2);

            builder.Property(wt => wt.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(wt => wt.ReferenceId)
                .HasMaxLength(100);

            builder.Property(wt => wt.Status)
                .HasConversion<int>();

            //  Use Restrict to avoid cascade paths
            builder.HasOne(wt => wt.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(wt => wt.WalletId)
                .OnDelete(DeleteBehavior.Restrict);  // Changed from Cascade to Restrict

            builder.HasIndex(wt => wt.WalletId);
            builder.HasIndex(wt => wt.ReferenceId);
            builder.HasIndex(wt => wt.CreatedAt);
            builder.HasIndex(wt => wt.Status);
        }
    }
}
