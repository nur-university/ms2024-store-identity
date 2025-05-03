using Identity.Application.Services;
using Identity.Infrastructure.Config;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.StoredModel;
using Identity.Infrastructure.Services;
using Joseco.Communication.External.RabbitMQ.Services;
using Joseco.CommunicationExternal.RabbitMQ;
using Joseco.DDD.Core.Abstractions;
using Joseco.Outbox.EFCore;
using Joseco.Outbox.EFCore.Persistence;
using Joseco.Secrets.Contrats;
using Joseco.Secrets.HashicorpVault;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nur.Store2025.Access.Identity.Permissions;
using Nur.Store2025.Security;
using Nur.Store2025.Security.Config;

namespace Identity.Infrastructure;

public static class DependencyInjection
{
   

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, 
        IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddVault()
            .AddDatabase()
            .AddOutbox<DomainEvent>()
            .AddSecurityServices(environment)
            .AddRabbitMQ();


        if (environment is IWebHostEnvironment)
        {            
            services.AddServicesHealthChecks();
        }

        return services;
    }

    private static IServiceCollection AddVault(this IServiceCollection services)
    {
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

    private static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        var databaseSettings = services.BuildServiceProvider().GetRequiredService<DataBaseSettings>();
        var connectionString = databaseSettings.ConnectionString;

        services.AddDbContext<SecurityDbContext>(ctx => ctx.UseNpgsql(connectionString));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            options.Lockout.MaxFailedAccessAttempts = 5;
        }).AddEntityFrameworkStores<SecurityDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IDatabase, SecurityDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOutboxDatabase<DomainEvent>, UnitOfWork>();

        return services;
    }

    private static IServiceCollection AddSecurityServices(this IServiceCollection services, IHostEnvironment environment)
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

    private static IServiceCollection AddRabbitMQ(this IServiceCollection services)
    {
        using var serviceProvider = services.BuildServiceProvider();
        var rabbitMqSettings = serviceProvider.GetRequiredService<RabbitMqSettings>();

        services.AddRabbitMQ(rabbitMqSettings);

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

    private static void LoadSecretsFromVault(this IServiceCollection services)
    {
        string jwtOptionsSecretName = "JwtOptions";
        string rabbitMqSettingsSecretName = "RabbitMqSettings";
        string identityDatabaseConnectionStringSecretName = "IdentityDatabaseConnectionString";
        string userInitializationSecretName = "UserInitialization";

        string vaultMountPoint = "secrets";

        using var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        using var scope = scopeFactory.CreateScope();
        var secretManager = scope.ServiceProvider.GetRequiredService<ISecretManager>();

        Task[] tasks = [
                LoadAndRegister<JwtOptions>(secretManager, services, jwtOptionsSecretName, vaultMountPoint),
                LoadAndRegister<RabbitMqSettings>(secretManager, services, rabbitMqSettingsSecretName, vaultMountPoint),
                LoadAndRegister<DataBaseSettings>(secretManager, services, identityDatabaseConnectionStringSecretName, vaultMountPoint),
                LoadAndRegister<InitializerJsonConfig>(secretManager, services, userInitializationSecretName, vaultMountPoint)
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
}

