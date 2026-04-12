using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Domain.Exceptions
{
    public class BusinessRuleViolationException : DomainException
    {
        public string? RuleName { get; private set; }

        public BusinessRuleViolationException()
            : base()
        {
        }

        public BusinessRuleViolationException(string message)
            : base(message)
        {
        }

        public BusinessRuleViolationException(string message, string ruleName)
            : base(message)
        {
            RuleName = ruleName;
        }

        public BusinessRuleViolationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public BusinessRuleViolationException(string message, string ruleName, Exception innerException)
            : base(message, innerException)
        {
            RuleName = ruleName;
        }
    }
}
