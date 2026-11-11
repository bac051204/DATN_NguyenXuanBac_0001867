using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalesInventory.Infrastructure.Persistence;

namespace SalesInventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // The connection string is NOT stored in appsettings.json. In Development it comes from user-secrets:
        //   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection string>" --project src\SalesInventory.API
        // In deployed environments it comes from the ConnectionStrings__DefaultConnection environment variable.
        services.AddDbContext<AppDbContext>(o =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "ConnectionStrings:DefaultConnection is not configured. Run: " +
                    "dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"<connection string>\" " +
                    "--project src\\SalesInventory.API (or set the ConnectionStrings__DefaultConnection environment variable).");
            }
            o.UseSqlServer(connectionString);
        });

        return services;
    }
}
