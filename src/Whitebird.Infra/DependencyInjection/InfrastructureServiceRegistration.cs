using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.AssetTransaction;
using Whitebird.Infra.Features.Auth;
using Whitebird.Infra.Features.Category;
using Whitebird.Infra.Features.Employee;
using Whitebird.Infra.Features.Supplier;
using Whitebird.Infra.Features.Reports;
using Whitebird.Infra.Features.ActivityLog;
using Whitebird.Infra.Features.MasterData;
using Whitebird.Infra.Features.Department;
using Whitebird.Infra.Features.Office;
using Whitebird.Infra.Features.FileAttachment;
using Whitebird.Migrations.Features.ActivityLog;

namespace Whitebird.Infra.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // ========== DATABASE ==========
        services.AddDatabaseServices(configuration);

        // ========== FLUENT MIGRATOR ==========
        services
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSqlServer2016()
                .WithGlobalConnectionString(configuration.GetConnectionString("DefaultConnection"))
                .ScanIn(typeof(CreateTableActivityLog).Assembly).For.Migrations())
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        // ========== GENERIC REPOSITORY ==========
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // ========== SPECIFIC REPOSITORIES ==========
        // Core modules
        services.AddScoped<IAssetReps, AssetReps>();
        services.AddScoped<IAssetTransactionReps, AssetTransactionReps>();
        services.AddScoped<IAuthReps, AuthReps>();
        services.AddScoped<ICategoryReps, CategoryReps>();
        services.AddScoped<IEmployeeReps, EmployeeReps>();
        services.AddScoped<ISupplierReps, SupplierReps>();
        services.AddScoped<IReportsReps, ReportsReps>();
        services.AddScoped<IActivityLogReps, ActivityLogReps>();

        // NEW MODULES
        services.AddScoped<IMasterDataReps, MasterDataReps>();
        services.AddScoped<IDepartmentReps, DepartmentReps>();
        services.AddScoped<IOfficeReps, OfficeReps>();
        services.AddScoped<IFileAttachmentReps, FileAttachmentReps>();

        return services;
    }
}