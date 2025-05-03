using Identity.Application;
using Identity.Infrastructure;
using Identity.WorkerService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
