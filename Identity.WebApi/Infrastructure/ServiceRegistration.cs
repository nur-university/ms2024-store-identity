
using Consul;

namespace Identity.WebApi.Infrastructure;

public class ServiceRegistration(ConsulClient consulClient, IConfiguration configuration, ILogger<ServiceRegistration> logger) : IHostedService
{
    private string _registrationId = string.Empty;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        string? serviceAddress = configuration.GetValue<string?>("ServiceRegistration:Host");
        int? servicePort = configuration.GetValue<int?>("ServiceRegistration:Port");
        string? serviceName = configuration.GetValue<string?>("ServiceRegistration:ServiceId");

        var registration = new AgentServiceRegistration
        {
            ID = $"{serviceName}-{Guid.NewGuid()}",
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
        _registrationId = registration.ID;
        logger.LogInformation("Registrando servicio en Consul...");
        await consulClient.Agent.ServiceRegister(registration, cancellationToken);

    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Desregistrando servicio de Consul...");
        if (!string.IsNullOrEmpty(_registrationId))
        {
            await consulClient.Agent.ServiceDeregister(_registrationId, cancellationToken);
        }
        logger.LogInformation("Servicio desregistrado.");
    }
}
