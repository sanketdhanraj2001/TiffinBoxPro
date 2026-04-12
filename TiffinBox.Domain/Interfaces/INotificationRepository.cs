using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Entities;

namespace TiffinBox.Domain.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<IReadOnlyList<Notification>> GetUnreadByUserAsync(Guid userId);
        Task<IReadOnlyList<Notification>> GetByUserAsync(Guid userId, int page, int pageSize);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadAsync(Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);
    }
}
