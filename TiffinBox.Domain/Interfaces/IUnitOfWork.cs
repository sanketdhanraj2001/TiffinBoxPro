using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Domain.Interfaces
{
    public interface IUnitOfWork:IDisposable
    {
        IUserRepository Users { get; }
        IVendorRepository Vendors { get; }
        ISubscriptionRepository Subscriptions { get; }
        ISubscriptionPlanRepository SubscriptionPlans { get; }
        IMenuItemRepository MenuItems { get; }  
        IReviewRepository Reviews { get; }
        IOrderRepository Orders { get; }
        IPaymentRepository Payments { get; }
        IWalletRepository Wallets { get; }
        INotificationRepository Notifications { get; }

        Task<int> CompleteAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
