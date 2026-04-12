using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Application.Common.Interfaces;
using TiffinBox.Infrastructure.Services.Authentication;

namespace TiffinBox.Infrastructure
{
    public class DependencyInjection
    {
        public static void RegisterServices(WebApplicationBuilder builder)
        {
            builder.Services.AddHttpContextAccessor();

            // Register CurrentUserService
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        }
    }
}
