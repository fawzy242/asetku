using Microsoft.Extensions.DependencyInjection;
using Whitebird.App.Features.Asset;
using Whitebird.App.Features.AssetTransaction;
using Whitebird.App.Features.Auth;
using Whitebird.App.Features.Category;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Department;
using Whitebird.App.Features.Employee;
using Whitebird.App.Features.FileAttachment;
using Whitebird.App.Features.MasterData;
using Whitebird.App.Features.Office;
using Whitebird.App.Features.Reports;
using Whitebird.App.Features.Supplier;

namespace Whitebird.App.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        // Common Services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();

        // Core Services
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IAssetTransactionService, AssetTransactionService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IReportsService, ReportsService>();

        // New Modules
        services.AddScoped<IMasterDataService, MasterDataService>();
        services.AddScoped<IMasterDataLookupService, MasterDataLookupService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IOfficeService, OfficeService>();
        services.AddScoped<IFileAttachmentService, FileAttachmentService>();
        services.AddScoped<IStorageService, StorageService>();

        // Import Services
        services.AddScoped<AssetImportService>();
        services.AddScoped<EmployeeImportService>();
        services.AddScoped<TransactionImportService>();
        return services;
    }
}