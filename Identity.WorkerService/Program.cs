using Identity.Application;
using Identity.Infrastructure;
using Joseco.DDD.Core.Abstractions;
using Joseco.Outbox.EFCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddOutboxBackgroundService<DomainEvent>();

var host = builder.Build();
host.Run();
