using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whitebird.Infra.Database;
using Whitebird.Infra.Configuration;

namespace Whitebird.Infra.DependencyInjection;

public static class DatabaseServiceRegistration
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));
        services.AddSingleton<IDatabaseConnectionFactory, DatabaseConnectionFactory>();
        services.AddScoped<DapperContext>();
        services.AddScoped<SqlHelper>();
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        return services;
    }
}