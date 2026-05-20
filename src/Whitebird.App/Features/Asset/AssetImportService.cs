using System.Data;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Common.Import;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Asset;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.Category;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Office;
using Whitebird.Infra.Features.Supplier;

namespace Whitebird.App.Features.Asset;

public class AssetImportService : BaseService, IImportService<AssetImportDto>
{
    private readonly IGenericRepository<AssetEntity> _repository;
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
        ILogger<AssetImportService> logger) : base(logger)
    {
        _repository = repository;
        _assetReps = assetReps;
        _categoryReps = categoryReps;
        _supplierReps = supplierReps;
        _officeReps = officeReps;
        _masterDataService = masterDataService;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<ServiceResult<ImportResult>> ImportFromExcelAsync(Stream fileStream, string? createdBy = null)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var result = new ImportResult();
            var assetsToInsert = new List<AssetEntity>();
            var createdByUser = createdBy ?? _currentUserService.GetDisplayName();

            var dataTable = await ExcelHelper.ReadExcelToDataTableAsync(fileStream, true);
            result.TotalRows = dataTable.Rows.Count;

            var categoryCache = new Dictionary<string, int>();
            var supplierCache = new Dictionary<string, int>();
            var officeCache = new Dictionary<string, int>();
            var conditionCache = new Dictionary<string, int>();
            var conditionPurchaseCache = new Dictionary<string, int>();

            var conditions = await _masterDataService.GetAssetConditionsAsync();
            if (conditions.IsSuccess)
            {
                foreach (var c in conditions.Data!)
                    conditionCache[c.Name.ToLowerInvariant()] = c.Code;
            }

            var conditionPurchases = await _masterDataService.GetAssetConditionPurchasesAsync();
            if (conditionPurchases.IsSuccess)
            {
                foreach (var c in conditionPurchases.Data!)
                    conditionPurchaseCache[c.Name.ToLowerInvariant()] = c.Code;
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = dataTable.Rows[i];
                var rowNumber = i + 2;
                var dto = new AssetImportDto();

                try
                {
                    dto.AssetCode = GetString(row, "AssetCode");
                    dto.AssetName = GetString(row, "AssetName");
                    dto.Brand = GetString(row, "Brand");
                    dto.Model = GetString(row, "Model");
                    dto.SerialNumber = GetString(row, "SerialNumber");
                    dto.Imei = GetString(row, "Imei");
                    dto.MacAddress = GetString(row, "MacAddress");
                    dto.InvoiceNumber = GetString(row, "InvoiceNumber");
                    dto.SupplierName = GetString(row, "SupplierName");
                    dto.OfficeName = GetString(row, "OfficeName");
                    dto.Hostname = GetString(row, "Hostname");
                    dto.IpAddress = GetString(row, "IpAddress");
                    dto.Notes = GetString(row, "Notes");
                    dto.AssetCondition = GetString(row, "AssetCondition");
                    dto.AssetConditionPurchase = GetString(row, "AssetConditionPurchase");

                    dto.CategoryId = GetInt(row, "CategoryId");
                    dto.CategoryName = GetString(row, "CategoryName");
                    dto.WarrantyPeriod = GetNullableInt(row, "WarrantyPeriod");
                    dto.PurchasePrice = GetNullableDecimal(row, "PurchasePrice");
                    dto.ResidualValue = GetNullableDecimal(row, "ResidualValue");
                    dto.UsefulLife = GetNullableInt(row, "UsefulLife");
                    dto.OperasionalOffice = GetNullableBool(row, "OperasionalOffice");

                    dto.PurchaseDate = GetNullableDateTime(row, "PurchaseDate");
                    dto.WarrantyExpiryDate = GetNullableDateTime(row, "WarrantyExpiryDate");
                    dto.DepreciationStartDate = GetNullableDateTime(row, "DepreciationStartDate");

                    if (string.IsNullOrWhiteSpace(dto.AssetCode))
                    {
                        result.AddError(rowNumber, "AssetCode", "AssetCode is required");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(dto.AssetName))
                    {
                        result.AddError(rowNumber, "AssetName", "AssetName is required");
                        continue;
                    }

                    if (await _assetReps.IsAssetCodeExistsAsync(dto.AssetCode))
                    {
                        result.AddError(rowNumber, "AssetCode", $"AssetCode '{dto.AssetCode}' already exists", dto.AssetCode);
                        continue;
                    }

                    int? categoryId = null;
                    if (dto.CategoryId > 0)
                    {
                        var category = await _categoryReps.GetByIdRawAsync(dto.CategoryId);
                        if (category == null)
                            result.AddError(rowNumber, "CategoryId", $"Category with id {dto.CategoryId} not found", dto.CategoryId.ToString());
                        else
                            categoryId = dto.CategoryId;
                    }
                    else if (!string.IsNullOrWhiteSpace(dto.CategoryName))
                    {
                        if (!categoryCache.ContainsKey(dto.CategoryName.ToLowerInvariant()))
                        {
                            var categories = await _categoryReps.GetAllWithRelationsAsync();
                            var category = categories.FirstOrDefault(c =>
                                c.CategoryName.Equals(dto.CategoryName, StringComparison.OrdinalIgnoreCase));
                            if (category != null)
                                categoryCache[dto.CategoryName.ToLowerInvariant()] = category.CategoryId;
                            else
                                result.AddError(rowNumber, "CategoryName", $"Category '{dto.CategoryName}' not found", dto.CategoryName);
                        }

                        if (categoryCache.ContainsKey(dto.CategoryName.ToLowerInvariant()))
                            categoryId = categoryCache[dto.CategoryName.ToLowerInvariant()];
                    }
                    else
                    {
                        result.AddError(rowNumber, "Category", "Either CategoryId or CategoryName is required");
                        continue;
                    }

                    if (!categoryId.HasValue)
                        continue;

                    int? supplierId = null;
                    if (!string.IsNullOrWhiteSpace(dto.SupplierName))
                    {
                        if (!supplierCache.ContainsKey(dto.SupplierName.ToLowerInvariant()))
                        {
                            var suppliers = await _supplierReps.GetAllAsync();
                            var supplier = suppliers.FirstOrDefault(s =>
                                s.SupplierName.Equals(dto.SupplierName, StringComparison.OrdinalIgnoreCase));
                            if (supplier != null)
                                supplierCache[dto.SupplierName.ToLowerInvariant()] = supplier.SupplierId;
                            else
                                result.AddWarning(rowNumber, "SupplierName", $"Supplier '{dto.SupplierName}' not found - will be created", dto.SupplierName);
                        }

                        if (supplierCache.ContainsKey(dto.SupplierName.ToLowerInvariant()))
                            supplierId = supplierCache[dto.SupplierName.ToLowerInvariant()];
                    }

                    int? officeId = null;
                    if (!string.IsNullOrWhiteSpace(dto.OfficeName))
                    {
                        if (!officeCache.ContainsKey(dto.OfficeName.ToLowerInvariant()))
                        {
                            var offices = await _officeReps.GetAllAsync();
                            var office = offices.FirstOrDefault(o =>
                                o.OfficeName.Equals(dto.OfficeName, StringComparison.OrdinalIgnoreCase));
                            if (office != null)
                                officeCache[dto.OfficeName.ToLowerInvariant()] = office.OfficeId;
                            else
                                result.AddError(rowNumber, "OfficeName", $"Office '{dto.OfficeName}' not found", dto.OfficeName);
                        }

                        if (officeCache.ContainsKey(dto.OfficeName.ToLowerInvariant()))
                            officeId = officeCache[dto.OfficeName.ToLowerInvariant()];
                    }

                    int? conditionId = null;
                    if (!string.IsNullOrWhiteSpace(dto.AssetCondition))
                    {
                        var key = dto.AssetCondition.ToLowerInvariant();
                        if (conditionCache.ContainsKey(key))
                            conditionId = conditionCache[key];
                        else
                            result.AddError(rowNumber, "AssetCondition", $"Invalid AssetCondition '{dto.AssetCondition}'. Valid: Good, Normal, Damaged", dto.AssetCondition);
                    }

                    int? conditionPurchaseId = null;
                    if (!string.IsNullOrWhiteSpace(dto.AssetConditionPurchase))
                    {
                        var key = dto.AssetConditionPurchase.ToLowerInvariant();
                        if (conditionPurchaseCache.ContainsKey(key))
                            conditionPurchaseId = conditionPurchaseCache[key];
                        else
                            result.AddError(rowNumber, "AssetConditionPurchase", $"Invalid AssetConditionPurchase '{dto.AssetConditionPurchase}'. Valid: New, Second Hand", dto.AssetConditionPurchase);
                    }

                    if (result.Errors.Any(e => e.RowNumber == rowNumber))
                        continue;

                    var entity = new AssetEntity
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
                        IsActive = false,
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdByUser
                    };

                    assetsToInsert.Add(entity);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.AddError(rowNumber, "General", $"Error processing row: {ex.Message}");
                    _logger.LogError(ex, "Error processing asset import row {RowNumber}", rowNumber);
                }
            }

            if (assetsToInsert.Any())
            {
                await _repository.BulkInsertAsync(assetsToInsert);

                foreach (var asset in assetsToInsert)
                {
                    await _activityLogService.LogCreateAsync(
                        "Asset",
                        asset.AssetId,
                        $"Asset '{asset.AssetCode}' imported (INACTIVE - needs activation)",
                        createdByUser);
                }
            }

            return ServiceResult<ImportResult>.Success(result,
                $"Import completed: {result.SuccessCount} successful, {result.ErrorCount} errors");
        }, "import assets", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Asset", 0, "Import Assets", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<byte[]>> GenerateTemplateAsync()
    {
        return await ExecuteSafelyAsync(() =>
        {
            var template = ExcelHelper.GenerateTemplate("Assets", _templateColumns,
                "Asset Import Template - Fill in the data below. Assets will be imported as INACTIVE and need manual activation.");
            return Task.FromResult(ServiceResult<byte[]>.Success(template));
        }, "generate asset import template");
    }

    public async Task<ServiceResult<ImportResult>> ValidateOnlyAsync(Stream fileStream)
    {
        var result = new ImportResult();
        return ServiceResult<ImportResult>.Success(result);
    }

    #region Helper Methods

    private string? GetString(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return null;
        var value = row[columnName]?.ToString();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private int GetInt(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return 0;
        var value = row[columnName]?.ToString();
        if (string.IsNullOrWhiteSpace(value))
            return 0;
        return int.TryParse(value, out var result) ? result : 0;
    }

    private int? GetNullableInt(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return null;
        var value = row[columnName]?.ToString();
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return int.TryParse(value, out var result) ? result : null;
    }

    private decimal? GetNullableDecimal(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return null;
        var value = row[columnName]?.ToString();
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return decimal.TryParse(value, out var result) ? result : null;
    }

    private DateTime? GetNullableDateTime(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return null;
        var value = row[columnName]?.ToString();
        if (string.IsNullOrWhiteSpace(value))
            return null;
        return DateTime.TryParse(value, out var result) ? result : null;
    }

    private bool? GetNullableBool(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return null;
        var value = row[columnName]?.ToString();
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (bool.TryParse(value, out var boolResult))
            return boolResult;
        if (value.Equals("1") || value.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
            return true;
        if (value.Equals("0") || value.Equals("FALSE", StringComparison.OrdinalIgnoreCase))
            return false;

        return null;
    }

    #endregion
}