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
using Whitebird.Domain.Features.FileAttachment;

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

        // ========== ASSET MAPPINGS ==========
        TypeAdapterConfig<AssetEntity, AssetDetailViewModel>.NewConfig()
            .Map(dest => dest.CategoryName, src => src.CategoryName ?? "Unknown")
            .Map(dest => dest.SupplierName, src => src.SupplierName ?? "Unknown")
            .Map(dest => dest.OfficeName, src => src.OfficeName ?? "Unknown")
            .Map(dest => dest.AssetConditionName, src => src.AssetConditionName ?? "Unknown")
            .Map(dest => dest.AssetConditionPurchaseName, src => src.AssetConditionPurchaseName ?? "Unknown");

        TypeAdapterConfig<AssetEntity, AssetListViewModel>.NewConfig()
            .Map(dest => dest.CategoryName, src => src.CategoryName ?? "Unknown")
            .Map(dest => dest.AssetConditionName, src => src.AssetConditionName ?? "Unknown")
            .Map(dest => dest.OfficeName, src => src.OfficeName ?? "Unknown");

        TypeAdapterConfig<AssetCreateViewModel, AssetEntity>.NewConfig()
            .Ignore(dest => dest.AssetId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy)
            .Ignore(dest => dest.LastMaintenanceDate)
            .Ignore(dest => dest.NextMaintenanceDate);

        TypeAdapterConfig<AssetUpdateViewModel, AssetEntity>.NewConfig()
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy);

        // ========== ASSET TRANSACTION MAPPINGS ==========
        TypeAdapterConfig<AssetTransactionEntity, AssetTransactionDetailViewModel>.NewConfig()
            .Map(dest => dest.AssetCode, src => src.AssetCode ?? "Unknown")
            .Map(dest => dest.AssetName, src => src.AssetName ?? "Unknown")
            .Map(dest => dest.FromEmployeeName, src => src.FromEmployeeName ?? "Unknown")
            .Map(dest => dest.ToEmployeeName, src => src.ToEmployeeName ?? "Unknown")
            .Map(dest => dest.ToLocationName, src => src.ToLocationName ?? "Unknown")
            .Map(dest => dest.TransactionTypeName, src => src.TransactionTypeName ?? "Unknown")
            .Map(dest => dest.ConditionBeforeName, src => src.ConditionBeforeName ?? "Unknown")
            .Map(dest => dest.ConditionAfterName, src => src.ConditionAfterName ?? "Unknown")
            .Map(dest => dest.MaintenanceTypeName, src => src.MaintenanceTypeName ?? "Unknown");

        TypeAdapterConfig<AssetTransactionEntity, AssetTransactionListViewModel>.NewConfig()
            .Map(dest => dest.AssetCode, src => src.AssetCode ?? "Unknown")
            .Map(dest => dest.AssetName, src => src.AssetName ?? "Unknown")
            .Map(dest => dest.FromEmployeeName, src => src.FromEmployeeName ?? "Unknown")
            .Map(dest => dest.ToEmployeeName, src => src.ToEmployeeName ?? "Unknown")
            .Map(dest => dest.ToLocationName, src => src.ToLocationName ?? "Unknown")
            .Map(dest => dest.TransactionTypeName, src => src.TransactionTypeName ?? "Unknown");

        TypeAdapterConfig<AssetTransactionCreateViewModel, AssetTransactionEntity>.NewConfig()
            .Ignore(dest => dest.AssetTransactionId)
            .Ignore(dest => dest.ActualReturnDate)
            .Ignore(dest => dest.Approved)
            .Ignore(dest => dest.ApprovedBy)
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

        // ========== CATEGORY MAPPINGS ==========
        TypeAdapterConfig<CategoryEntity, CategoryDetailViewModel>.NewConfig()
            .Map(dest => dest.ParentCategoryName, src => src.ParentCategoryName ?? "Unknown");

        TypeAdapterConfig<CategoryEntity, CategoryListViewModel>.NewConfig()
            .Map(dest => dest.ParentCategoryName, src => src.ParentCategoryName ?? "Unknown");

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

        // ========== EMPLOYEE MAPPINGS ==========
        TypeAdapterConfig<EmployeeEntity, EmployeeDetailViewModel>.NewConfig()
            .Ignore(dest => dest.ActiveAssetsCount)
            .Ignore(dest => dest.PositionName)
            .Ignore(dest => dest.EmploymentStatusName)
            .Ignore(dest => dest.DepartmentName)
            .Ignore(dest => dest.OfficeName);

        TypeAdapterConfig<EmployeeEntity, EmployeeListViewModel>.NewConfig()
            .Ignore(dest => dest.PositionName)
            .Ignore(dest => dest.EmploymentStatusName)
            .Ignore(dest => dest.DepartmentName)
            .Ignore(dest => dest.OfficeName);

        TypeAdapterConfig<EmployeeCreateViewModel, EmployeeEntity>.NewConfig()
            .Ignore(dest => dest.EmployeeId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        TypeAdapterConfig<EmployeeUpdateViewModel, EmployeeEntity>.NewConfig()
            .Ignore(dest => dest.EmployeeId)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy);

        // ========== SUPPLIER MAPPINGS ==========
        TypeAdapterConfig<SupplierEntity, SupplierDetailViewModel>.NewConfig()
            .Ignore(dest => dest.AssetCount);

        TypeAdapterConfig<SupplierEntity, SupplierListViewModel>.NewConfig()
            .Ignore(dest => dest.AssetCount);

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

        // ========== MASTER DATA MAPPINGS ==========
        TypeAdapterConfig<MasterDataEntity, MasterDataDetailViewModel>.NewConfig();
        TypeAdapterConfig<MasterDataEntity, MasterDataListViewModel>.NewConfig();

        // ========== DEPARTMENT MAPPINGS ==========
        TypeAdapterConfig<DepartmentEntity, DepartmentDetailViewModel>.NewConfig()
            .Ignore(dest => dest.EmployeeCount);

        TypeAdapterConfig<DepartmentEntity, DepartmentListViewModel>.NewConfig()
            .Ignore(dest => dest.EmployeeCount);

        TypeAdapterConfig<DepartmentCreateViewModel, DepartmentEntity>.NewConfig()
            .Ignore(dest => dest.DepartmentId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        TypeAdapterConfig<DepartmentUpdateViewModel, DepartmentEntity>.NewConfig()
            .Ignore(dest => dest.DepartmentId)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy);

        // ========== OFFICE MAPPINGS (DIPERBAIKI) ==========
        TypeAdapterConfig<OfficeEntity, OfficeDetailViewModel>.NewConfig()
            .Map(dest => dest.ParentOfficeName, src => src.ParentOfficeName ?? "Unknown")
            .Ignore(dest => dest.OfficeTypeName)
            .Ignore(dest => dest.ChildCount);

        TypeAdapterConfig<OfficeEntity, OfficeListViewModel>.NewConfig()
            .Map(dest => dest.ParentOfficeName, src => src.ParentOfficeName ?? "Unknown")
            .Ignore(dest => dest.OfficeTypeName);

        TypeAdapterConfig<OfficeCreateViewModel, OfficeEntity>.NewConfig()
            .Ignore(dest => dest.OfficeId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        TypeAdapterConfig<OfficeUpdateViewModel, OfficeEntity>.NewConfig()
            .Ignore(dest => dest.OfficeId)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy);

        // ========== FILE ATTACHMENT MAPPINGS ==========
        TypeAdapterConfig<FileAttachmentEntity, FileAttachmentDetailViewModel>.NewConfig();
        TypeAdapterConfig<FileAttachmentEntity, FileAttachmentListViewModel>.NewConfig();
    }
}