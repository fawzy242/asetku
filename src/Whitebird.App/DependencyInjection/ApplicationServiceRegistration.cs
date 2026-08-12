using Microsoft.Extensions.DependencyInjection;
using Whitebird.App.Features.Asset;
using Whitebird.App.Features.AssetTracking;
using Whitebird.App.Features.AssetTransaction;
using Whitebird.App.Features.Auth;
using Whitebird.App.Features.Category;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Common.Export;
using Whitebird.App.Features.Dashboard;
using Whitebird.App.Features.Department;
using Whitebird.App.Features.Employee;
using Whitebird.App.Features.EmployeeSummary;
using Whitebird.App.Features.FileAttachment;
using Whitebird.App.Features.Home;
using Whitebird.App.Features.MasterData;
using Whitebird.App.Features.Office;
using Whitebird.App.Features.Reports;
using Whitebird.App.Features.Reports.Draw;
using Whitebird.App.Features.Schedule;
using Whitebird.App.Features.Supplier;
using Whitebird.App.Features.User;

namespace Whitebird.App.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        // Common Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();

        // ========== GENERIC SERVICES ==========
        // Export Service - Generic
        services.AddScoped(typeof(IExportService<>), typeof(ExportService<>));

        // ========== CORE SERVICES ==========
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IAssetTransactionService, AssetTransactionService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IReportsService, ReportsService>();

        // ========== NEW MODULES ==========
        services.AddScoped<IMasterDataService, MasterDataService>();
        services.AddScoped<IMasterDataLookupService, MasterDataLookupService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IOfficeService, OfficeService>();
        services.AddScoped<IFileAttachmentService, FileAttachmentService>();
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<IUserService, UserService>();

        // ========== SCHEDULE ==========
        services.AddScoped<IScheduleHandoverService, ScheduleHandoverService>();
        services.AddScoped<IScheduleReturnService, ScheduleReturnService>();
        services.AddScoped<IScheduleMaintenanceService, ScheduleMaintenanceService>();

        // ========== DASHBOARD, TRACKING, SUMMARY ==========
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAssetTrackingService, AssetTrackingService>();
        services.AddScoped<IEmployeeSummaryService, EmployeeSummaryService>();

        // ========== HOME ==========
        services.AddScoped<IHomeService, HomeService>();

        // ========== DRAW REPORTS ==========
        services.AddScoped<DrawReportAssetTransaction>();
        services.AddScoped<DrawReportAssetInventory>();
        services.AddScoped<DrawReportEmployeeAsset>();
        services.AddScoped<DrawReportMaintenance>();
        services.AddScoped<DrawReportFinancial>();

        // ========== IMPORT SERVICES ==========
        services.AddScoped<AssetImportService>();
        services.AddScoped<EmployeeImportService>();
        services.AddScoped<TransactionImportService>();

        return services;
    }
}