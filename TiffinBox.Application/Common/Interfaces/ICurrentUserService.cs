using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        int GetCurrentUserId();
        string? GetUserRole();
        string? GetUserEmail();
        string? GetUserPhone();
        string? GetUserFullName();
        bool IsAuthenticated();
        bool IsInRole(string role);
    }
}
