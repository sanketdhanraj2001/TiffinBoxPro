using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.DTOs.Auth
{
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;  // ✅ Make sure this exists
        public string Password { get; set; } = string.Empty;  // ✅ Make sure this exists
    }
}
