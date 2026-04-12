using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.Common.Settings
{
    public class SmsSettings
    {
        public string AccountSid { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string FromPhoneNumber { get; set; } = string.Empty;
        public string Provider { get; set; } = "Twilio"; // Twilio, AWS SNS, Vonage, etc.
    }
}
