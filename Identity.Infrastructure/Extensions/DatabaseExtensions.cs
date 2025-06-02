using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.StoredModel;
using Joseco.DDD.Core.Abstractions;
using Joseco.Outbox.Contracts.Service;
using Joseco.Outbox.EFCore.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Joseco.Outbox.EFCore;

namespace Identity.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
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
        services.AddOutbox<DomainEvent>();

        services.Decorate<IOutboxService<DomainEvent>, OutboxTracingService<DomainEvent>>();

        return services;
    }
}
