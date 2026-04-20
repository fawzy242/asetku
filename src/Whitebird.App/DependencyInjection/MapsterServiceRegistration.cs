using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Whitebird.Domain.Features.Asset.Entities;
using Whitebird.Domain.Features.Asset.View;
using Whitebird.Domain.Features.AssetTransaction.Entities;
using Whitebird.Domain.Features.AssetTransaction.View;
using Whitebird.Domain.Features.Category.Entities;
using Whitebird.Domain.Features.Category.View;
using Whitebird.Domain.Features.Employee.Entities;
using Whitebird.Domain.Features.Employee.View;
using Whitebird.Domain.Features.Location.Entities;
using Whitebird.Domain.Features.Location.View;
using Whitebird.Domain.Features.Supplier.Entities;
using Whitebird.Domain.Features.Supplier.View;

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
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);

        // Asset Mappings
        TypeAdapterConfig<AssetEntity, AssetDetailViewModel>.NewConfig()
            .Map(dest => dest.CategoryName, src => src.CategoryName ?? "Unknown")
            .Map(dest => dest.CurrentHolderName, src => src.CurrentHolderName ?? "Unknown")
            .Map(dest => dest.ResponsiblePartyName, src => src.ResponsiblePartyName ?? "Unknown")
            .Map(dest => dest.SupplierName, src => src.SupplierName ?? "Unknown");

        TypeAdapterConfig<AssetCreateViewModel, AssetEntity>.NewConfig()
            .Ignore(dest => dest.AssetId)
            .Ignore(dest => dest.AssetCode)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        TypeAdapterConfig<AssetUpdateViewModel, AssetEntity>.NewConfig()
            .Ignore(dest => dest.AssetCode)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy);

        // AssetTransaction Mappings
        TypeAdapterConfig<AssetTransactionCreateViewModel, AssetTransactionEntity>.NewConfig()
            .Ignore(dest => dest.AssetTransactionId)
            .Ignore(dest => dest.ActualReturnDate)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        TypeAdapterConfig<AssetTransactionUpdateViewModel, AssetTransactionEntity>.NewConfig()
            .Ignore(dest => dest.AssetTransactionId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy);

        // Category Mappings
        TypeAdapterConfig<CategoryCreateViewModel, CategoryEntity>.NewConfig()
            .Ignore(dest => dest.CategoryId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        TypeAdapterConfig<CategoryUpdateViewModel, CategoryEntity>.NewConfig()
            .Ignore(dest => dest.CategoryId)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy);

        // Employee Mappings
        TypeAdapterConfig<EmployeeCreateViewModel, EmployeeEntity>.NewConfig()
            .Ignore(dest => dest.EmployeeId)
            .Ignore(dest => dest.EmployeeCode)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        TypeAdapterConfig<EmployeeUpdateViewModel, EmployeeEntity>.NewConfig()
            .Ignore(dest => dest.EmployeeId)
            .Ignore(dest => dest.EmployeeCode)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy);

        // Location Mappings
        TypeAdapterConfig<LocationCreateViewModel, LocationEntity>.NewConfig()
            .Ignore(dest => dest.LocationId)
            .Ignore(dest => dest.LocationCode)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        TypeAdapterConfig<LocationUpdateViewModel, LocationEntity>.NewConfig()
            .Ignore(dest => dest.LocationId)
            .Ignore(dest => dest.LocationCode)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy);

        // Supplier Mappings
        TypeAdapterConfig<SupplierCreateViewModel, SupplierEntity>.NewConfig()
            .Ignore(dest => dest.SupplierId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        TypeAdapterConfig<SupplierUpdateViewModel, SupplierEntity>.NewConfig()
            .Ignore(dest => dest.SupplierId)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy);
    }
}