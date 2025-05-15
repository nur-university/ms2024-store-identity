using Identity.Infrastructure.Config;
using Identity.Infrastructure.Persistence;
using Joseco.Communication.External.RabbitMQ.Services;
using Joseco.Secrets.Contrats;
using Joseco.Secrets.HashicorpVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nur.Store2025.Security.Config;

namespace Identity.Infrastructure.Extensions;

public static class SecretExtensions
{
    public static IServiceCollection AddSecrets(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        bool useSecretManager = configuration.GetValue<bool>("UseSecretManager", false);
        if (environment.IsDevelopment() && !useSecretManager)
        {
            string jwtOptionsSecretName = "JwtOptions";
            string rabbitMqSettingsSecretName = "RabbitMqSettings";
            string identityDatabaseConnectionStringSecretName = "IdentityDatabaseConnectionString";
            string defaultUserSecretName = "DefaultUser";

            configuration
                .LoadAndRegister<JwtOptions>(services, jwtOptionsSecretName)
                .LoadAndRegister<RabbitMqSettings>(services, rabbitMqSettingsSecretName)
                .LoadAndRegister<DataBaseSettings>(services, identityDatabaseConnectionStringSecretName)
                .LoadAndRegister<InitializerJsonUser>(services, defaultUserSecretName);

            return services;
        }

        string? vaultUrl = Environment.GetEnvironmentVariable("VAULT_URL");
        string? vaultToken = Environment.GetEnvironmentVariable("VAULT_TOKEN");

        if (string.IsNullOrEmpty(vaultUrl) || string.IsNullOrEmpty(vaultToken))
        {
            throw new InvalidOperationException("Vault URL or Token is not set in environment variables.");
        }

        var settings = new VaultSettings()
        {
            VaultUrl = vaultUrl,
            VaultToken = vaultToken
        };

        services.AddHashicorpVault(settings)
            .LoadSecretsFromVault();

        return services;
    }

    private static void LoadSecretsFromVault(this IServiceCollection services)
    {
        string jwtOptionsSecretName = "JwtOptions";
        string rabbitMqSettingsSecretName = "RabbitMqSettings";
        string identityDatabaseConnectionStringSecretName = "IdentityDatabaseConnectionString";
        string defaultUserSecretName = "DefaultUser";

        string vaultMountPoint = "secrets";

        using var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        using var scope = scopeFactory.CreateScope();
        var secretManager = scope.ServiceProvider.GetRequiredService<ISecretManager>();

        Task[] tasks = [
                LoadAndRegister<JwtOptions>(secretManager, services, jwtOptionsSecretName, vaultMountPoint),
                LoadAndRegister<RabbitMqSettings>(secretManager, services, rabbitMqSettingsSecretName, vaultMountPoint),
                LoadAndRegister<DataBaseSettings>(secretManager, services, identityDatabaseConnectionStringSecretName, vaultMountPoint),
                LoadAndRegister<InitializerJsonUser>(secretManager, services, defaultUserSecretName, vaultMountPoint)
            ];


        Task.WaitAll(tasks);
    }

    private static async Task LoadAndRegister<T>(ISecretManager secretManager, IServiceCollection services,
        string secretName, string mountPoint) where T : class, new()
    {
        T secret = await secretManager.Get<T>(secretName, mountPoint);
        secret ??= Activator.CreateInstance<T>();
        services.AddSingleton<T>(secret);
    }

    private static IConfiguration LoadAndRegister<T>(this IConfiguration configuration, IServiceCollection services,
        string secretName) where T : class, new()
    {
        T secret = Activator.CreateInstance<T>();
        configuration.Bind(secretName, secret);
        services.AddSingleton<T>(secret);
        return configuration;
    }
}
