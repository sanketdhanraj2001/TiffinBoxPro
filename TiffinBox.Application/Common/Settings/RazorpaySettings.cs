using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.Common.Settings
{
    public class RazorpaySettings
    {
        public string KeyId { get; set; } = string.Empty;
        public string KeySecret { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
    }
}
