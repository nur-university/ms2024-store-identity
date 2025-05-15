using Identity.Infrastructure.Extensions;
using Joseco.CommunicationExternal.RabbitMQ;
using Joseco.DDD.Core.Abstractions;
using Joseco.Outbox.EFCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
   

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, 
        IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddSecrets(configuration, environment)
            .AddDatabase()
            .AddOutbox<DomainEvent>()
            .AddSecurityServices(environment)
            .AddRabbitMQ()
            .AddObservability(environment);

        return services;
    }
}

