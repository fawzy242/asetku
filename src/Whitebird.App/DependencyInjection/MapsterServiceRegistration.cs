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
using Whitebird.App.Features.Asset;
using Whitebird.App.Features.AssetTransaction;
using Whitebird.App.Features.Category;
using Whitebird.App.Features.Employee;
using Whitebird.App.Features.Supplier;
using Whitebird.App.Features.Department;
using Whitebird.App.Features.Office;
using Whitebird.App.Features.FileAttachment;
using Whitebird.App.Features.MasterData;

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
        TypeAdapterConfig<AssetListView, AssetListViewModel>.NewConfig()
            .Map(dest => dest.AssetId, src => src.AssetId)
            .Map(dest => dest.AssetCode, src => src.AssetCode ?? string.Empty)
            .Map(dest => dest.AssetName, src => src.AssetName ?? string.Empty)
            .Map(dest => dest.CategoryName, src => src.CategoryName ?? "Unknown")
            .Map(dest => dest.Brand, src => src.Brand)
            .Map(dest => dest.Model, src => src.Model)
            .Map(dest => dest.PurchaseDate, src => src.PurchaseDate)
            .Map(dest => dest.PurchasePrice, src => src.PurchasePrice)
            .Map(dest => dest.IsActive, src => src.IsActive);

        TypeAdapterConfig<AssetCreateViewModel, AssetEntity>.NewConfig()
            .Ignore(dest => dest.AssetId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        TypeAdapterConfig<AssetUpdateViewModel, AssetEntity>.NewConfig()
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy);

        // ========== ASSET TRANSACTION MAPPINGS ==========
        TypeAdapterConfig<AssetTransactionListView, AssetTransactionListViewModel>.NewConfig()
            .Map(dest => dest.AssetTransactionId, src => src.AssetTransactionId)
            .Map(dest => dest.AssetId, src => src.AssetId)
            .Map(dest => dest.AssetCode, src => src.AssetCode ?? "Unknown")
            .Map(dest => dest.AssetName, src => src.AssetName ?? "Unknown")
            .Map(dest => dest.TransactionType, src => src.TransactionType)
            .Map(dest => dest.TransactionTypeName, src => src.TransactionTypeName ?? "Unknown")
            .Map(dest => dest.EmployeeName, src => src.EmployeeName)
            .Map(dest => dest.OfficeName, src => src.OfficeName)
            .Map(dest => dest.TransactionDate, src => src.TransactionDate)
            .Map(dest => dest.Approved, src => src.Approved)
            .Map(dest => dest.ExpectedLoanReturnDate, src => src.ExpectedLoanReturnDate)
            .Map(dest => dest.FromAssetTransactionId, src => src.FromAssetTransactionId)
            .Map(dest => dest.IsOverdue, src => src.IsOverdue);

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
        TypeAdapterConfig<CategoryListView, CategoryListViewModel>.NewConfig()
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.CategoryCode, src => src.CategoryCode)
            .Map(dest => dest.CategoryName, src => src.CategoryName ?? string.Empty)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.ParentCategoryId, src => src.ParentCategoryId)
            .Map(dest => dest.ParentCategoryName, src => src.ParentCategoryName)
            .Map(dest => dest.ChildCount, src => src.ChildCount)
            .Map(dest => dest.IsActive, src => src.IsActive);

        TypeAdapterConfig<CategoryCreateViewModel, CategoryEntity>.NewConfig()
            .Ignore(dest => dest.CategoryId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        // ========== EMPLOYEE MAPPINGS ==========
        TypeAdapterConfig<EmployeeListView, EmployeeListViewModel>.NewConfig()
            .Map(dest => dest.EmployeeId, src => src.EmployeeId)
            .Map(dest => dest.EmployeeCode, src => src.EmployeeCode ?? string.Empty)
            .Map(dest => dest.FullName, src => src.FullName ?? string.Empty)
            .Map(dest => dest.DepartmentName, src => src.DepartmentName)
            .Map(dest => dest.PositionName, src => src.PositionName)
            .Map(dest => dest.EmploymentStatusName, src => src.EmploymentStatusName)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.OfficeName, src => src.OfficeName)
            .Map(dest => dest.IsActive, src => src.IsActive);

        TypeAdapterConfig<EmployeeCreateViewModel, EmployeeEntity>.NewConfig()
            .Ignore(dest => dest.EmployeeId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        // ========== SUPPLIER MAPPINGS ==========
        TypeAdapterConfig<SupplierListView, SupplierListViewModel>.NewConfig()
            .Map(dest => dest.SupplierId, src => src.SupplierId)
            .Map(dest => dest.SupplierName, src => src.SupplierName ?? string.Empty)
            .Map(dest => dest.ContactPerson, src => src.ContactPerson)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.AssetCount, src => src.AssetCount);

        TypeAdapterConfig<SupplierCreateViewModel, SupplierEntity>.NewConfig()
            .Ignore(dest => dest.SupplierId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        // ========== DEPARTMENT MAPPINGS ==========
        TypeAdapterConfig<DepartmentListView, DepartmentListViewModel>.NewConfig()
            .Map(dest => dest.DepartmentId, src => src.DepartmentId)
            .Map(dest => dest.DepartmentCode, src => src.DepartmentCode)
            .Map(dest => dest.DepartmentName, src => src.DepartmentName ?? string.Empty)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.IsActive, src => src.IsActive)
            .Map(dest => dest.EmployeeCount, src => src.EmployeeCount);

        TypeAdapterConfig<DepartmentCreateViewModel, DepartmentEntity>.NewConfig()
            .Ignore(dest => dest.DepartmentId)
            .Ignore(dest => dest.IsActive)
            .Ignore(dest => dest.CreatedDate)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.ModifiedDate)
            .Ignore(dest => dest.ModifiedBy);

        // ========== OFFICE MAPPINGS ==========
        TypeAdapterConfig<OfficeListView, OfficeListViewModel>.NewConfig()
            .Map(dest => dest.OfficeId, src => src.OfficeId)
            .Map(dest => dest.OfficeCode, src => src.OfficeCode)
            .Map(dest => dest.OfficeName, src => src.OfficeName ?? string.Empty)
            .Map(dest => dest.OfficeType, src => src.OfficeType)
            .Map(dest => dest.OfficeTypeName, src => src.OfficeTypeName)
            .Map(dest => dest.City, src => src.City)
            .Map(dest => dest.ParentOfficeId, src => src.ParentOfficeId)
            .Map(dest => dest.ParentOfficeName, src => src.ParentOfficeName)
            .Map(dest => dest.IsActive, src => src.IsActive);

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
    }
}