using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.Common.Settings
{
    public class SmsSettings
    {
        // 2Factor settings
        public string Provider { get; set; } = "2Factor";
        public string ApiKey { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;

        public string TemplateName { get; set; } = string.Empty;


        // Keep existing properties for other providers
        public string SMSAccountIdentification { get; set; } = string.Empty;
        public string SMSAccountPassword { get; set; } = string.Empty;
        public string SMSAccountFrom { get; set; } = string.Empty;
    }
}
