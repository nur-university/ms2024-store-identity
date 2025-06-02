using Identity.Application;
using Identity.Infrastructure;
using Identity.WebApi;
using Identity.WebApi.Extensions;
using Nur.Store2025.Observability;

var builder = WebApplication.CreateBuilder(args);

string serviceName = "identity.api";

builder.Host.UseLogging(serviceName, builder.Configuration);

// Add services to the container.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment, serviceName)
    .AddPresentation(builder.Configuration, builder.Environment);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithUi();
    app.ApplyMigrations(app.Environment);
}


app.ApplySecurity(app.Environment);

app.UseRouting();

app.UseHealthChecks();

app.UseRequestContextLogging();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
