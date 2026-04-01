using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get;  set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; protected set; } // only child classes can modify it 
        public bool IsDeleted { get; protected set; } = false;

        protected void UpdateTimestamp() => UpdatedAt = DateTime.UtcNow;

        public void SoftDelete()
        {
            IsDeleted = true;
            UpdateTimestamp();
        }
    }
}
