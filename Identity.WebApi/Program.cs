using Identity.Application;
using Identity.Infrastructure;
using Identity.WebApi;
using Identity.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Environment)
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
