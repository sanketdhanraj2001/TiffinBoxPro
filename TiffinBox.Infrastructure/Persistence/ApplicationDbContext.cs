using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TiffinBox.Domain.Common;
using TiffinBox.Domain.Entities;
using TiffinBox.Domain.ValueObjects;

namespace TiffinBox.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Vendor> Vendors => Set<Vendor>();
        public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<SubscriptionMeal> SubscriptionMeals => Set<SubscriptionMeal>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<DeliveryAgent> DeliveryAgents => Set<DeliveryAgent>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
        public DbSet<Notification> Notifications => Set<Notification>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // ✔ call base first

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Global query filter for soft delete
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                    var condition = Expression.Equal(property, Expression.Constant(false));
                    var lambda = Expression.Lambda(condition, parameter);

                    entityType.SetQueryFilter(lambda);
                }
            }

            // Shared empty list (better performance + safe)
            var emptyList = new List<string>();

            modelBuilder.Entity<Review>()
                .Property(r => r.Images)
                .HasConversion(
                    v => JsonSerializer.Serialize(v ?? emptyList, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? emptyList
                )
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (c1, c2) =>
                        (c1 ?? emptyList).SequenceEqual(c2 ?? emptyList),

                    c => (c ?? emptyList)
                        .Aggregate(0, (a, v) => HashCode.Combine(a, v != null ? v.GetHashCode() : 0)),

                    c => (c ?? emptyList).ToList()
                ));

            // FIX decimal warnings (IMPORTANT)
            modelBuilder.Entity<Payment>()
                .Property(p => p.RefundAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Money>()
                .Property(m => m.Amount)
                .HasPrecision(18, 2);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
