using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whitebird.Infra.Database;
using Whitebird.Infra.Configuration;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.AssetTransaction;
using Whitebird.Infra.Features.Auth;
using Whitebird.Infra.Features.Category;
using Whitebird.Infra.Features.Employee;
using Whitebird.Infra.Features.Location;
using Whitebird.Infra.Features.Supplier;
using Whitebird.Infra.Features.Reports;
using Whitebird.Infra.Features.ActivityLog;

namespace Whitebird.Infra.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabaseServices(configuration);
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IAssetReps, AssetReps>();
        services.AddScoped<IAssetTransactionReps, AssetTransactionReps>();
        services.AddScoped<IAuthReps, AuthReps>();
        services.AddScoped<ICategoryReps, CategoryReps>();
        services.AddScoped<IEmployeeReps, EmployeeReps>();
        services.AddScoped<ILocationReps, LocationReps>();
        services.AddScoped<ISupplierReps, SupplierReps>();
        services.AddScoped<IReportsReps, ReportsReps>();
        services.AddScoped<IActivityLogReps, ActivityLogReps>();
        return services;
    }
}