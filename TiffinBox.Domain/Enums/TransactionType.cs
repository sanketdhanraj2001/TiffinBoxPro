using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Domain.Enums
{
    public enum TransactionType
    {
        Credit = 1,
        Debit = 2
    }
    public enum TransactionStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4
    }
}
