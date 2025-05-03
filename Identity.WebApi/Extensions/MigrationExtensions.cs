using Identity.Infrastructure.Config;
using Identity.Infrastructure.Persistence;

namespace Identity.WebApi.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app, IWebHostEnvironment env )
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using var db =
            scope.ServiceProvider.GetRequiredService<IDatabase>();

        db.Migrate();

    }

    public static void ApplySecurity(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        ISecurityInitializer securityInitializer = scope.ServiceProvider.GetRequiredService<ISecurityInitializer>();
        string contentRootPath = env.ContentRootPath;
        var permissionJsonFilePath = contentRootPath + "/DataFiles/permissions.json";
        var securityInitializationJsonFilePath = contentRootPath + "/DataFiles/initializer.json";
        securityInitializer.Initialize(permissionJsonFilePath, securityInitializationJsonFilePath).GetAwaiter().GetResult();
    }
}
