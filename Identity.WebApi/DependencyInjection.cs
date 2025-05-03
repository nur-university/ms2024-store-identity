using Identity.WebApi.Infrastructure;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Consul;

namespace Identity.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // REMARK: If you want to use Controllers, you'll need this.
        services.AddControllers();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        if (!environment.IsDevelopment())
        {
            services.RegisterServiceToServiceDiscovery(configuration);
        }         

        return services;
    }

    private static IServiceCollection RegisterServiceToServiceDiscovery(this IServiceCollection services, IConfiguration configuration)
    {
       
        string? serviceDiscoveryAddress = configuration.GetValue<string?>("ServiceRegistration:ServiceDiscoveryAddress");

        services.AddSingleton(sp => new ConsulClient(c => c.Address = new Uri(serviceDiscoveryAddress)));
        services.AddHostedService<ServiceRegistration>();

        return services;
    }
}
