using System.Data;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Common.Import;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Asset;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.Category;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Office;
using Whitebird.Infra.Features.Supplier;

namespace Whitebird.App.Features.Asset;

public class AssetImportService : ImportServiceBase<AssetImportDto, AssetEntity>
{
    private readonly IAssetReps _assetReps;
    private readonly ICategoryReps _categoryReps;
    private readonly ISupplierReps _supplierReps;
    private readonly IOfficeReps _officeReps;
    private readonly IMasterDataService _masterDataService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    private readonly Dictionary<string, string> _templateColumns = new()
    {
        { "AssetCode", "Required. Unique asset code (max 50 chars)" },
        { "AssetName", "Required. Asset name (max 100 chars)" },
        { "CategoryId", "Required. Numeric category ID from Category table" },
        { "CategoryName", "Optional. Alternative to CategoryId - will lookup by name" },
        { "Brand", "Optional. Brand name (max 50 chars)" },
        { "Model", "Optional. Model name (max 50 chars)" },
        { "SerialNumber", "Optional. Serial number (max 50 chars)" },
        { "Imei", "Optional. IMEI number (max 50 chars)" },
        { "MacAddress", "Optional. MAC address (max 50 chars)" },
        { "PurchaseDate", "Optional. Format: YYYY-MM-DD" },
        { "PurchasePrice", "Optional. Numeric value" },
        { "InvoiceNumber", "Optional. Invoice reference (max 50 chars)" },
        { "SupplierName", "Optional. Supplier name - will lookup or create" },
        { "WarrantyPeriod", "Optional. Warranty period in months" },
        { "AssetCondition", "Optional. Values: Good, Normal, Damaged" },
        { "AssetConditionPurchase", "Optional. Values: New, Second Hand" },
        { "OfficeName", "Optional. Office name - will lookup" },
        { "Hostname", "Optional. Hostname (max 50 chars)" },
        { "IpAddress", "Optional. IP address (max 50 chars)" },
        { "OperasionalOffice", "Optional. TRUE/FALSE" },
        { "ResidualValue", "Optional. Numeric value" },
        { "UsefulLife", "Optional. Useful life in years" },
        { "DepreciationStartDate", "Optional. Format: YYYY-MM-DD" },
        { "Notes", "Optional. Additional notes (max 500 chars)" }
    };

    public AssetImportService(
        IGenericRepository<AssetEntity> repository,
        IAssetReps assetReps,
        ICategoryReps categoryReps,
        ISupplierReps supplierReps,
        IOfficeReps officeReps,
        IMasterDataService masterDataService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<AssetImportService> logger)
        : base(repository, currentUserService, activityLogService, logger, TableNames.Asset, "Asset")
    {
        _assetReps = assetReps;
        _categoryReps = categoryReps;
        _supplierReps = supplierReps;
        _officeReps = officeReps;
        _masterDataService = masterDataService;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    protected override Dictionary<string, string> GetTemplateColumns() => _templateColumns;

    protected override async Task InitializeCachesAsync(Dictionary<string, object> cache)
    {
        var categoryDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var allCategories = await _categoryReps.GetAllListViewAsync();
        foreach (var c in allCategories)
        {
            categoryDict[c.CategoryName] = c.CategoryId;
        }
        cache["Categories"] = categoryDict;

        var supplierDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var allSuppliers = await _supplierReps.GetAllListViewAsync();
        foreach (var s in allSuppliers)
        {
            supplierDict[s.SupplierName] = s.SupplierId;
        }
        cache["Suppliers"] = supplierDict;

        var officeDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var allOffices = await _officeReps.GetAllListViewAsync();
        foreach (var o in allOffices)
        {
            officeDict[o.OfficeName] = o.OfficeId;
        }
        cache["Offices"] = officeDict;

        var conditionCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var conditions = await _masterDataService.GetAssetConditionsAsync();
        if (conditions.IsSuccess && conditions.Data != null)
        {
            foreach (var c in conditions.Data)
                conditionCache[c.Name] = c.Code;
        }
        cache["Conditions"] = conditionCache;

        var conditionPurchaseCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var conditionPurchases = await _masterDataService.GetAssetConditionPurchasesAsync();
        if (conditionPurchases.IsSuccess && conditionPurchases.Data != null)
        {
            foreach (var c in conditionPurchases.Data)
                conditionPurchaseCache[c.Name] = c.Code;
        }
        cache["ConditionPurchases"] = conditionPurchaseCache;
    }

    protected override async Task<AssetEntity?> ProcessRowAsync(
        DataRow row,
        int rowNumber,
        ImportResult result,
        Dictionary<string, object> cache,
        string createdBy)
    {
        var dto = new AssetImportDto
        {
            AssetCode = ExcelDataReader.GetString(row, "AssetCode") ?? string.Empty,
            AssetName = ExcelDataReader.GetString(row, "AssetName") ?? string.Empty,
            Brand = ExcelDataReader.GetString(row, "Brand"),
            Model = ExcelDataReader.GetString(row, "Model"),
            SerialNumber = ExcelDataReader.GetString(row, "SerialNumber"),
            Imei = ExcelDataReader.GetString(row, "Imei"),
            MacAddress = ExcelDataReader.GetString(row, "MacAddress"),
            InvoiceNumber = ExcelDataReader.GetString(row, "InvoiceNumber"),
            SupplierName = ExcelDataReader.GetString(row, "SupplierName"),
            OfficeName = ExcelDataReader.GetString(row, "OfficeName"),
            Hostname = ExcelDataReader.GetString(row, "Hostname"),
            IpAddress = ExcelDataReader.GetString(row, "IpAddress"),
            Notes = ExcelDataReader.GetString(row, "Notes"),
            AssetCondition = ExcelDataReader.GetString(row, "AssetCondition"),
            AssetConditionPurchase = ExcelDataReader.GetString(row, "AssetConditionPurchase"),
            CategoryId = ExcelDataReader.GetInt(row, "CategoryId"),
            CategoryName = ExcelDataReader.GetString(row, "CategoryName"),
            WarrantyPeriod = ExcelDataReader.GetNullableInt(row, "WarrantyPeriod"),
            PurchasePrice = ExcelDataReader.GetNullableDecimal(row, "PurchasePrice"),
            ResidualValue = ExcelDataReader.GetNullableDecimal(row, "ResidualValue"),
            UsefulLife = ExcelDataReader.GetNullableInt(row, "UsefulLife"),
            OperasionalOffice = ExcelDataReader.GetNullableBool(row, "OperasionalOffice"),
            PurchaseDate = ExcelDataReader.GetNullableDateTime(row, "PurchaseDate"),
            WarrantyExpiryDate = ExcelDataReader.GetNullableDateTime(row, "WarrantyExpiryDate"),
            DepreciationStartDate = ExcelDataReader.GetNullableDateTime(row, "DepreciationStartDate")
        };

        if (string.IsNullOrWhiteSpace(dto.AssetCode))
        {
            result.AddError(rowNumber, "AssetCode", "AssetCode is required");
            return null;
        }

        if (string.IsNullOrWhiteSpace(dto.AssetName))
        {
            result.AddError(rowNumber, "AssetName", "AssetName is required");
            return null;
        }

        if (await _assetReps.IsAssetCodeExistsAsync(dto.AssetCode))
        {
            result.AddError(rowNumber, "AssetCode", $"AssetCode '{dto.AssetCode}' already exists", dto.AssetCode);
            return null;
        }

        var categoryDict = cache["Categories"] as Dictionary<string, int>;
        var supplierDict = cache["Suppliers"] as Dictionary<string, int>;
        var officeDict = cache["Offices"] as Dictionary<string, int>;
        var conditionCache = cache["Conditions"] as Dictionary<string, int>;
        var conditionPurchaseCache = cache["ConditionPurchases"] as Dictionary<string, int>;

        int? categoryId = null;
        if (dto.CategoryId > 0)
        {
            var category = await _categoryReps.GetByIdRawAsync(dto.CategoryId);
            if (category == null)
                result.AddError(rowNumber, "CategoryId", $"Category with id {dto.CategoryId} not found", dto.CategoryId.ToString());
            else
                categoryId = dto.CategoryId;
        }
        else if (!string.IsNullOrWhiteSpace(dto.CategoryName) && categoryDict != null)
        {
            if (categoryDict.TryGetValue(dto.CategoryName, out int catId))
                categoryId = catId;
            else
                result.AddError(rowNumber, "CategoryName", $"Category '{dto.CategoryName}' not found", dto.CategoryName);
        }
        else
        {
            result.AddError(rowNumber, "Category", "Either CategoryId or CategoryName is required");
            return null;
        }

        if (!categoryId.HasValue)
            return null;

        int? supplierId = null;
        if (!string.IsNullOrWhiteSpace(dto.SupplierName) && supplierDict != null)
        {
            if (!supplierDict.TryGetValue(dto.SupplierName, out int supId))
                result.AddWarning(rowNumber, "SupplierName", $"Supplier '{dto.SupplierName}' not found - will be created", dto.SupplierName);
            else
                supplierId = supId;
        }

        int? officeId = null;
        if (!string.IsNullOrWhiteSpace(dto.OfficeName) && officeDict != null)
        {
            if (!officeDict.TryGetValue(dto.OfficeName, out int offId))
                result.AddError(rowNumber, "OfficeName", $"Office '{dto.OfficeName}' not found", dto.OfficeName);
            else
                officeId = offId;
        }

        int? conditionId = null;
        if (!string.IsNullOrWhiteSpace(dto.AssetCondition) && conditionCache != null)
        {
            if (!conditionCache.TryGetValue(dto.AssetCondition, out int condId))
                result.AddError(rowNumber, "AssetCondition", $"Invalid AssetCondition '{dto.AssetCondition}'. Valid: Good, Normal, Damaged", dto.AssetCondition);
            else
                conditionId = condId;
        }

        int? conditionPurchaseId = null;
        if (!string.IsNullOrWhiteSpace(dto.AssetConditionPurchase) && conditionPurchaseCache != null)
        {
            if (!conditionPurchaseCache.TryGetValue(dto.AssetConditionPurchase, out int condId))
                result.AddError(rowNumber, "AssetConditionPurchase", $"Invalid AssetConditionPurchase '{dto.AssetConditionPurchase}'. Valid: New, Second Hand", dto.AssetConditionPurchase);
            else
                conditionPurchaseId = condId;
        }

        if (result.Errors.Any(e => e.RowNumber == rowNumber))
            return null;

        // Operational Office Rule: If OperasionalOffice = true, OfficeId is required
        if (dto.OperasionalOffice == true && !officeId.HasValue)
        {
            result.AddError(rowNumber, "OfficeName", "Office is required when Operational Office is enabled");
            return null;
        }

        return new AssetEntity
        {
            AssetCode = dto.AssetCode,
            AssetName = dto.AssetName,
            CategoryId = categoryId.Value,
            Brand = dto.Brand,
            Model = dto.Model,
            SerialNumber = dto.SerialNumber,
            Imei = dto.Imei,
            MacAddress = dto.MacAddress,
            PurchaseDate = dto.PurchaseDate,
            PurchasePrice = dto.PurchasePrice,
            InvoiceNumber = dto.InvoiceNumber,
            SupplierId = supplierId,
            WarrantyPeriod = dto.WarrantyPeriod,
            WarrantyExpiryDate = dto.WarrantyExpiryDate,
            AssetCondition = conditionId,
            AssetConditionPurchase = conditionPurchaseId,
            ResidualValue = dto.ResidualValue,
            UsefulLife = dto.UsefulLife,
            DepreciationStartDate = dto.DepreciationStartDate,
            Notes = dto.Notes,
            OfficeId = officeId,
            Hostname = dto.Hostname,
            IpAddress = dto.IpAddress,
            OperasionalOffice = dto.OperasionalOffice ?? false,
            IsActive = true,
            CreatedDate = DateTime.Now,
            CreatedBy = createdBy
        };
    }

    protected override async Task<bool> IsEntityUniqueAsync(AssetEntity entity, ImportResult result, int rowNumber)
    {
        var exists = await _assetReps.IsAssetCodeExistsAsync(entity.AssetCode);
        if (exists)
        {
            result.AddError(rowNumber, "AssetCode", $"AssetCode '{entity.AssetCode}' already exists", entity.AssetCode);
            return false;
        }
        return true;
    }

    protected override string GetEntityIdentifier(AssetEntity entity) => entity.AssetCode;
}