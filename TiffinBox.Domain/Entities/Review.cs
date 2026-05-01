using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Domain.Common;
using TiffinBox.Domain.Exceptions;

namespace TiffinBox.Domain.Entities
{
    public class Review : BaseEntity
    {
        public int CustomerId { get; private set; }
        public virtual User Customer { get; private set; }
        public int VendorId { get; private set; }
        public virtual Vendor Vendor { get; private set; }
        public int? OrderId { get; private set; }
        public virtual Order? Order { get; private set; }
        public int Rating { get; private set; }
        public string? Comment { get; private set; }
        public string? Reply { get; private set; }
        public DateTime? RepliedAt { get; private set; }
        public bool IsVerified { get; private set; }
        public bool IsApproved { get; private set; }
        public List<string>? Images { get; private set; }

        private Review() { }

        public static Review Create(int customerId, int vendorId, int rating, string? comment = null, int? orderId = null)
        {
            if (rating < 1 || rating > 5)
                throw new BusinessRuleViolationException("Rating must be between 1 and 5");

            return new Review
            {
                CustomerId = customerId,
                VendorId = vendorId,
                OrderId = orderId,
                Rating = rating,
                Comment = comment,
                IsVerified = orderId.HasValue,
                IsApproved = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void UpdateReview(int? rating, string? comment)
        {
            if (rating.HasValue)
            {
                if (rating.Value < 1 || rating.Value > 5)
                    throw new BusinessRuleViolationException("Rating must be between 1 and 5");
                Rating = rating.Value;
            }

            if (comment != null)
                Comment = comment;

            UpdateTimestamp();
        }

        public void AddReply(string reply)
        {
            Reply = reply;
            RepliedAt = DateTime.UtcNow;
            UpdateTimestamp();
        }

        public void Approve()
        {
            IsApproved = true;
            UpdateTimestamp();
        }

        public void Reject()
        {
            IsApproved = false;
            UpdateTimestamp();
        }

        public void AddImages(List<string> imageUrls)
        {
            Images ??= new List<string>();
            Images.AddRange(imageUrls);
            UpdateTimestamp();
        }

        public bool IsCustomerReview(int customerId)
        {
            return CustomerId == customerId;
        }

        public bool IsVendorReview(int vendorId)
        {
            return VendorId == vendorId;
        }
    }
}
