using Identity.Application.Services;
using Identity.Infrastructure.Config;
using Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nur.Store2025.Access.Identity.Permissions;
using Nur.Store2025.Security;
using Nur.Store2025.Security.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Extensions;

public static class SecurityExtensions
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IHostEnvironment environment)
    {
        if (environment is IWebHostEnvironment)
        {
            var jwtOptions = services.BuildServiceProvider().GetRequiredService<JwtOptions>();
            services.AddSecurity(jwtOptions, IdentityPermission.PermissionsList);
        }

        services.AddScoped<ISecurityService, SecurityService>();
        services.AddScoped<ISecurityInitializer, SecurityInitializer>();

        return services;
    }
}
