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
using Whitebird.Infra.Features.Roles;
using Whitebird.Infra.Features.UserRoles;
using Whitebird.Migrations.Features.ActivityLog;
using Whitebird.Infra.Features.User;
using Whitebird.Infra.Features.Schedule;
using Whitebird.Infra.Features.Dashboard;
using Whitebird.Infra.Features.AssetTracking;
using Whitebird.Infra.Features.EmployeeSummary;
using Whitebird.Infra.Features.Home;

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

        // ========== GENERIC REPOSITORY PRIMITIVES ==========
        // Single abstraction replacing the previous IGenericRepository<T> + GenericRepository<T> pair.
        services.AddScoped<ICommonReps, CommonReps>();

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
        services.AddScoped<IUserReps, UserReps>();
        services.AddScoped<IRolesReps, RolesReps>();
        services.AddScoped<IUserRolesReps, UserRolesReps>();
        services.AddScoped<IScheduleHandoverReps, ScheduleHandoverReps>();
        services.AddScoped<IScheduleReturnReps, ScheduleReturnReps>();
        services.AddScoped<IScheduleMaintenanceReps, ScheduleMaintenanceReps>();
        services.AddScoped<IDashboardReps, DashboardReps>();
        services.AddScoped<IAssetTrackingReps, AssetTrackingReps>();
        services.AddScoped<IEmployeeSummaryReps, EmployeeSummaryReps>();
        services.AddScoped<IHomeReps, HomeReps>();
        return services;
    }
}