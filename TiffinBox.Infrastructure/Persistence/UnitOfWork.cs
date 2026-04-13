using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;
using TiffinBox.Domain.Interfaces;
using TiffinBox.Infrastructure.Persistence.Repositories;

namespace TiffinBox.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        private IUserRepository? _users;
        private IVendorRepository? _vendors;
        private IOrderRepository? _orders;
        private IPaymentRepository? _payments;
        private IWalletRepository? _wallets;
        private INotificationRepository? _notifications;
        private ISubscriptionRepository? _subscriptions;
        private ISubscriptionPlanRepository? _subscriptionPlans; 
        private IMenuItemRepository? _menuItems;  
        private IReviewRepository? _reviews;


        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IVendorRepository Vendors => _vendors ??= new VendorRepository(_context);
        public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
        public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);
        public IWalletRepository Wallets => _wallets ??= new WalletRepository(_context);
        public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_context);
        public ISubscriptionRepository Subscriptions => _subscriptions ??= new SubscriptionRepository(_context);
        public ISubscriptionPlanRepository SubscriptionPlans => _subscriptionPlans ??= new SubscriptionPlanRepository(_context);

        public IMenuItemRepository MenuItems => _menuItems ??= new MenuItemRepository(_context);
        public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);

        public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _transaction?.Dispose();
                _context.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}