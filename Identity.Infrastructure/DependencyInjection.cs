using Identity.Infrastructure.Extensions;
using Joseco.Communication.External.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
   

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, 
        IConfiguration configuration, IHostEnvironment environment, string serviceName)
    {
        services.AddSecrets(configuration, environment)
            .AddDatabase()
            .AddSecurityServices(environment)
            .AddRabbitMQ()
            .AddObservability(environment, serviceName);

        return services;
    }
}

