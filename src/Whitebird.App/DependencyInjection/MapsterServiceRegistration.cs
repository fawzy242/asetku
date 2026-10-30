using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Whitebird.Domain.Features.Asset;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Domain.Features.Category;
using Whitebird.Domain.Features.Employee;
using Whitebird.Domain.Features.Supplier;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Domain.Features.Department;
using Whitebird.Domain.Features.Office;
using Whitebird.Domain.Features.Roles;
using Whitebird.App.Features.Asset;
using Whitebird.App.Features.AssetTransaction;
using Whitebird.App.Features.Category;
using Whitebird.App.Features.Employee;
using Whitebird.App.Features.Supplier;
using Whitebird.App.Features.Department;
using Whitebird.App.Features.Office;
using Whitebird.App.Features.MasterData;
using Whitebird.App.Features.Roles;

namespace Whitebird.App.DependencyInjection;

public static class MapsterServiceRegistration
{
    public static IServiceCollection AddMapsterConfiguration(this IServiceCollection services)
    {
        services.AddMapster();
        ConfigureMappings();
        return services;
    }

    private static void ConfigureMappings()
    {
        // CRITICAL FIX: Do not ignore null values globally.
        // This allows null values from client to actually reset fields in the database.
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(false);

        // ========== ASSET MAPPINGS ==========

        TypeAdapterConfig<AssetCreateViewModel, AssetEntity>.NewConfig()
            .Ignore(dest => dest.AssetId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        // ========== ASSET TRANSACTION MAPPINGS ==========

        TypeAdapterConfig<AssetTransactionCreateViewModel, AssetTransactionEntity>.NewConfig()
            .Ignore(dest => dest.AssetTransactionId)
            .Ignore(dest => dest.Approved)
            .Ignore(dest => dest.ApprovedBy)
            .Ignore(dest => dest.Completed)
            .Ignore(dest => dest.CompletedBy)
            .Ignore(dest => dest.AssetTransactionCode)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        // ========== CATEGORY MAPPINGS ==========

        TypeAdapterConfig<CategoryCreateViewModel, CategoryEntity>.NewConfig()
            .Ignore(dest => dest.CategoryId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        // ========== EMPLOYEE MAPPINGS ==========

        TypeAdapterConfig<EmployeeCreateViewModel, EmployeeEntity>.NewConfig()
            .Ignore(dest => dest.EmployeeId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        // ========== SUPPLIER MAPPINGS ==========

        TypeAdapterConfig<SupplierCreateViewModel, SupplierEntity>.NewConfig()
            .Ignore(dest => dest.SupplierId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        // ========== DEPARTMENT MAPPINGS ==========

        TypeAdapterConfig<DepartmentCreateViewModel, DepartmentEntity>.NewConfig()
            .Ignore(dest => dest.DepartmentId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        // ========== OFFICE MAPPINGS ==========

        TypeAdapterConfig<OfficeCreateViewModel, OfficeEntity>.NewConfig()
            .Ignore(dest => dest.OfficeId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        // ========== MASTER DATA MAPPINGS ==========

        TypeAdapterConfig<MasterDataEntity, MasterDataDto>.NewConfig()
            .Map(dest => dest.Code, src => src.ReferenceCode)
            .Map(dest => dest.Name, src => src.MasterDataName ?? string.Empty);

        // ========== ROLES MAPPINGS ==========

        TypeAdapterConfig<RolesCreateViewModel, RolesEntity>.NewConfig()
            .Ignore(dest => dest.RoleId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);
    }
}