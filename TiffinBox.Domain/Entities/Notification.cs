using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Common;
using TiffinBox.Domain.Enums;

namespace TiffinBox.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; private set; }
        public virtual User User { get; private set; }
        public NotificationType Type { get; private set; }
        public string Title { get; private set; }
        public string Message { get; private set; }
        public string? Body { get; private set; }
        public NotificationChannel Channel { get; private set; }
        public bool IsRead { get; private set; }
        public DateTime? ReadAt { get; private set; }
        public string? ActionUrl { get; private set; }
        public string? ImageUrl { get; private set; }
        public Dictionary<string, string>? Data { get; private set; }
        public DateTime? SentAt { get; private set; }
        public DateTime? DeliveredAt { get; private set; }
        public string? ErrorMessage { get; private set; }
        public int RetryCount { get; private set; }

        private Notification() { }

        public static Notification Create(
            Guid userId,
            NotificationType type,
            string title,
            string message,
            NotificationChannel channel = NotificationChannel.InApp,
            string? actionUrl = null,
            string? imageUrl = null,
            Dictionary<string, string>? data = null)
        {
            return new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                Body = message,
                Channel = channel,
                IsRead = false,
                ActionUrl = actionUrl,
                ImageUrl = imageUrl,
                Data = data,
                SentAt = DateTime.UtcNow
            };
        }

        public void MarkAsRead()
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
            UpdateTimestamp();
        }

        public void MarkAsDelivered()
        {
            DeliveredAt = DateTime.UtcNow;
            UpdateTimestamp();
        }

        public void MarkAsFailed(string errorMessage)
        {
            ErrorMessage = errorMessage;
            UpdateTimestamp();
        }

        public void IncrementRetryCount()
        {
            RetryCount++;
            UpdateTimestamp();
        }

        public bool ShouldRetry()
        {
            return RetryCount < 3 && !IsRead;
        }
    }
}
