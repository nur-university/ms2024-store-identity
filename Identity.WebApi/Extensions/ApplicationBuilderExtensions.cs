using Consul;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Identity.WebApi.Extensions;
public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSwaggerWithUi(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        return app;
    }



    public static void UseHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    }

    public static void RegisterServiceToServiceDiscovery(this WebApplication app)
    {
        string? serviceAddress = app.Configuration.GetValue<string?>("ServiceRegistration:Host");
        int? servicePort = app.Configuration.GetValue<int?>("ServiceRegistration:Port");
        string? serviceName = app.Configuration.GetValue<string?>("ServiceRegistration:ServiceId");
        string? serviceDiscoveryAddress = app.Configuration.GetValue<string?>("ServiceRegistration:ServiceDiscoveryAddress");

        var consulClient = new ConsulClient(c => c.Address = new Uri(serviceDiscoveryAddress));

        var registration = new AgentServiceRegistration
        {
            ID = $"{serviceName}-{servicePort}",
            Name = serviceName,
            Address = serviceAddress,
            Port = servicePort == null ? 0 : servicePort.Value,
            Check = new AgentServiceCheck
            {
                HTTP = $"{serviceAddress}:{servicePort}/health",
                Interval = TimeSpan.FromSeconds(10),
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(30),
                Method = "GET"
            }
        };

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
            consulClient.Agent.ServiceRegister(registration).ConfigureAwait(true); ;
        });

        app.Lifetime.ApplicationStopping.Register(() =>
        {
            var resetEvent = new ManualResetEventSlim(false);
            Task.Run(async () =>
            {
                await consulClient.Agent.ServiceDeregister(registration.ID);
                resetEvent.Set();
            });

            resetEvent.Wait();
        });
    }
}
