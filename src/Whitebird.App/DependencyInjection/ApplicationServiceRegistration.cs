using Microsoft.Extensions.DependencyInjection;
using Whitebird.App.Features.Asset.Interfaces;
using Whitebird.App.Features.Asset.Service;
using Whitebird.App.Features.AssetTransaction.Interfaces;
using Whitebird.App.Features.AssetTransaction.Service;
using Whitebird.App.Features.Auth.Interfaces;
using Whitebird.App.Features.Auth.Service;
using Whitebird.App.Features.Category.Interfaces;
using Whitebird.App.Features.Category.Service;
using Whitebird.App.Features.Employee.Interfaces;
using Whitebird.App.Features.Employee.Service;
using Whitebird.App.Features.Location.Interfaces;
using Whitebird.App.Features.Location.Service;
using Whitebird.App.Features.Reports.Interfaces;
using Whitebird.App.Features.Reports.Service;
using Whitebird.App.Features.Supplier.Interfaces;
using Whitebird.App.Features.Supplier.Service;
using Whitebird.App.Features.Common.Service;

namespace Whitebird.App.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IAssetTransactionService, AssetTransactionService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IReportsService, ReportsService>();
        services.AddScoped<ISupplierService, SupplierService>();
        return services;
    }
}