using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Domain.Common
{
    public abstract class BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; protected set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get;  set; }
        public bool IsDeleted { get; protected set; } = false;
       // public byte[] RowVersion { get; protected set; } = null!;

        protected void UpdateTimestamp() => UpdatedAt = DateTime.UtcNow;

        public void SoftDelete()
        {
            IsDeleted = true;
            UpdateTimestamp();
        }

        public void Restore()
        {
            IsDeleted = false;
            UpdateTimestamp();
        }
    }
}
