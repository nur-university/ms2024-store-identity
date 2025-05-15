using Identity.Application.Services;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Services;
using Joseco.CommunicationExternal.RabbitMQ;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Extensions;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();

        if (environment is IWebHostEnvironment)
        {
            services.AddServicesHealthChecks();
        }
        return services;
    }
    private static IServiceCollection AddServicesHealthChecks(this IServiceCollection services)
    {
        var databaseSettings = services.BuildServiceProvider().GetRequiredService<DataBaseSettings>();
        string? dbConnectionString = databaseSettings.ConnectionString;

        services
            .AddHealthChecks()
            .AddNpgSql(dbConnectionString)
            .AddRabbitMqHealthCheck();

        return services;
    }
}
