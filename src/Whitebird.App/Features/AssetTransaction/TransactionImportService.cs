using System.Data;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Common.Import;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.AssetTransaction;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Employee;
using Whitebird.Infra.Features.Office;

namespace Whitebird.App.Features.AssetTransaction;

public class TransactionImportService : ImportService<TransactionImportDto, AssetTransactionEntity>
{
    private readonly IAssetTransactionReps _transactionReps;
    private readonly IAssetReps _assetReps;
    private readonly IEmployeeReps _employeeReps;
    private readonly IOfficeReps _officeReps;
    private readonly IMasterDataService _masterDataService;

    private readonly Dictionary<string, string> _templateColumns = new()
    {
        { "AssetCode", "Required. Asset code must exist in system" },
        { "TransactionType", "Required. Values: HANDOVER, TRANSFER, LOAN, MAINTENANCE, DISPOSAL, OPERATIONAL OFFICE" },
        { "EmployeeCode", "Optional. Employee code (receiver for HANDOVER/TRANSFER/LOAN)" },
        { "OfficeName", "Optional. Office name for OPERATIONAL OFFICE" },
        { "TransactionDate", "Required. Format: YYYY-MM-DD HH:MM:SS" },
        { "ExpectedLoanReturnDate", "Optional. Format: YYYY-MM-DD (required for LOAN)" },
        { "Notes", "Optional. Additional notes" },
        { "Condition", "Optional. Values: Good, Normal, Damaged" },
        { "MaintenanceType", "Optional. Values: PREVENTIVE MAINTENANCE, CORRECTIVE MAINTENANCE, EMERGENCY REPAIR, etc" },
        { "MaintenanceCost", "Optional. Numeric value" },
        { "FromAssetTransactionId", "Optional. For pairing LOAN_RETURN or POST_MAINTENANCE" }
    };

    public TransactionImportService(
        ICommonReps repository,
        IAssetTransactionReps transactionReps,
        IAssetReps assetReps,
        IEmployeeReps employeeReps,
        IOfficeReps officeReps,
        IMasterDataService masterDataService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<TransactionImportService> logger)
        : base(repository, currentUserService, activityLogService, logger, TableNames.AssetTransaction, "Transaction")
    {
        _transactionReps = transactionReps;
        _assetReps = assetReps;
        _employeeReps = employeeReps;
        _officeReps = officeReps;
        _masterDataService = masterDataService;
    }

    protected override Dictionary<string, string> GetTemplateColumns() => _templateColumns;

    protected override async Task InitializeCachesAsync(Dictionary<string, object> cache)
    {
        var assetCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var a in await _assetReps.GetAllListViewAsync()) assetCache[a.AssetCode] = a.AssetId;
        cache["Assets"] = assetCache;

        var employeeDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in await _employeeReps.GetDropdownListAsync()) employeeDict[e.EmployeeCode] = e.EmployeeId;
        cache["Employees"] = employeeDict;

        var officeDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var o in await _officeReps.GetAllListViewAsync()) officeDict[o.OfficeName] = o.OfficeId;
        cache["Offices"] = officeDict;

        var transactionTypeCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var transactionTypes = await _masterDataService.GetTransactionTypesAsync();
        if (transactionTypes.IsSuccess && transactionTypes.Data != null)
            foreach (var t in transactionTypes.Data) transactionTypeCache[t.Name] = t.Code;
        cache["TransactionTypes"] = transactionTypeCache;

        var conditionCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var conditions = await _masterDataService.GetAssetConditionsAsync();
        if (conditions.IsSuccess && conditions.Data != null)
            foreach (var c in conditions.Data) conditionCache[c.Name] = c.Code;
        cache["Conditions"] = conditionCache;

        var maintenanceTypeCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var maintenanceTypes = await _masterDataService.GetMaintenanceTypesAsync();
        if (maintenanceTypes.IsSuccess && maintenanceTypes.Data != null)
            foreach (var m in maintenanceTypes.Data) maintenanceTypeCache[m.Name] = m.Code;
        cache["MaintenanceTypes"] = maintenanceTypeCache;
    }

    protected override async Task<AssetTransactionEntity?> ProcessRowAsync(
        DataRow row, int rowNumber, ImportResult result, Dictionary<string, object> cache, string createdBy)
    {
        var assetCache = cache["Assets"] as Dictionary<string, int>;
        var employeeDict = cache["Employees"] as Dictionary<string, int>;
        var officeDict = cache["Offices"] as Dictionary<string, int>;
        var transactionTypeCache = cache["TransactionTypes"] as Dictionary<string, int>;
        var conditionCache = cache["Conditions"] as Dictionary<string, int>;
        var maintenanceTypeCache = cache["MaintenanceTypes"] as Dictionary<string, int>;

        var assetCode = ExcelDataReader.GetString(row, "AssetCode");
        var transactionTypeName = ExcelDataReader.GetString(row, "TransactionType");
        var employeeCode = ExcelDataReader.GetString(row, "EmployeeCode");
        var officeName = ExcelDataReader.GetString(row, "OfficeName");
        var notes = ExcelDataReader.GetString(row, "Notes");
        var conditionName = ExcelDataReader.GetString(row, "Condition");
        var maintenanceTypeName = ExcelDataReader.GetString(row, "MaintenanceType");
        var transactionDate = ExcelDataReader.GetDateTime(row, "TransactionDate");
        var expectedLoanReturnDate = ExcelDataReader.GetNullableDateTime(row, "ExpectedLoanReturnDate");
        var maintenanceCost = ExcelDataReader.GetNullableDecimal(row, "MaintenanceCost");
        var fromAssetTransactionId = ExcelDataReader.GetNullableInt(row, "FromAssetTransactionId");

        if (string.IsNullOrWhiteSpace(assetCode)) { result.AddError(rowNumber, "AssetCode", "AssetCode is required"); return null; }
        if (string.IsNullOrWhiteSpace(transactionTypeName)) { result.AddError(rowNumber, "TransactionType", "TransactionType is required"); return null; }
        if (transactionDate == DateTime.MinValue) { result.AddError(rowNumber, "TransactionDate", "TransactionDate is required"); return null; }

        if (assetCache == null || !assetCache.TryGetValue(assetCode, out int assetId))
        {
            result.AddError(rowNumber, "AssetCode", $"Asset '{assetCode}' not found", assetCode);
            return null;
        }

        if (transactionTypeCache == null || !transactionTypeCache.TryGetValue(transactionTypeName, out int transactionTypeId))
        {
            result.AddError(rowNumber, "TransactionType", $"Invalid TransactionType '{transactionTypeName}'", transactionTypeName);
            return null;
        }

        var addTypes = new[] {
            TransactionTypeConstants.HANDOVER, TransactionTypeConstants.TRANSFER, TransactionTypeConstants.LOAN,
            TransactionTypeConstants.MAINTENANCE, TransactionTypeConstants.DISPOSAL, TransactionTypeConstants.OPERATIONAL_OFFICE
        };

        if (!addTypes.Contains(transactionTypeId))
        {
            result.AddError(rowNumber, "TransactionType", $"Transaction type '{transactionTypeName}' cannot be imported. Only ADD types are allowed.");
            return null;
        }

        int? employeeId = null;
        if (!string.IsNullOrWhiteSpace(employeeCode) && employeeDict != null)
        {
            if (!employeeDict.TryGetValue(employeeCode, out int empId))
                result.AddError(rowNumber, "EmployeeCode", $"Employee '{employeeCode}' not found", employeeCode);
            else employeeId = empId;
        }

        int? officeId = null;
        if (!string.IsNullOrWhiteSpace(officeName) && officeDict != null)
        {
            if (!officeDict.TryGetValue(officeName, out int offId))
                result.AddError(rowNumber, "OfficeName", $"Office '{officeName}' not found", officeName);
            else officeId = offId;
        }

        if (transactionTypeId == TransactionTypeConstants.HANDOVER && !employeeId.HasValue)
        { result.AddError(rowNumber, "EmployeeCode", "HANDOVER requires EmployeeCode"); return null; }
        if (transactionTypeId == TransactionTypeConstants.TRANSFER && !employeeId.HasValue)
        { result.AddError(rowNumber, "EmployeeCode", "TRANSFER requires EmployeeCode (receiver)"); return null; }
        if (transactionTypeId == TransactionTypeConstants.LOAN)
        {
            if (!employeeId.HasValue) { result.AddError(rowNumber, "EmployeeCode", "LOAN requires EmployeeCode"); return null; }
            if (!expectedLoanReturnDate.HasValue) { result.AddError(rowNumber, "ExpectedLoanReturnDate", "LOAN requires ExpectedLoanReturnDate"); return null; }
        }
        if (transactionTypeId == TransactionTypeConstants.OPERATIONAL_OFFICE && !officeId.HasValue)
        { result.AddError(rowNumber, "OfficeName", "OPERATIONAL OFFICE requires OfficeName"); return null; }

        int? conditionId = null;
        if (!string.IsNullOrWhiteSpace(conditionName) && conditionCache != null)
        {
            if (!conditionCache.TryGetValue(conditionName, out int condId))
                result.AddError(rowNumber, "Condition", $"Invalid Condition '{conditionName}'. Valid: Good, Normal, Damaged", conditionName);
            else conditionId = condId;
        }

        int? maintenanceTypeId = null;
        if (!string.IsNullOrWhiteSpace(maintenanceTypeName) && maintenanceTypeCache != null)
        {
            if (!maintenanceTypeCache.TryGetValue(maintenanceTypeName, out int maintId))
                result.AddError(rowNumber, "MaintenanceType", $"Invalid MaintenanceType '{maintenanceTypeName}'", maintenanceTypeName);
            else maintenanceTypeId = maintId;
        }

        if (result.Errors.Any(e => e.RowNumber == rowNumber)) return null;

        return new AssetTransactionEntity
        {
            AssetId = assetId,
            TransactionType = transactionTypeId,
            EmployeeId = employeeId,
            OfficeId = officeId,
            TransactionDate = transactionDate,
            ExpectedLoanReturnDate = expectedLoanReturnDate,
            Notes = notes,
            Condition = conditionId,
            MaintenanceType = maintenanceTypeId,
            MaintenanceCost = maintenanceCost,
            FromAssetTransactionId = fromAssetTransactionId,
            Approved = null,
            IsActive = true,
            CreatedDate = DateTime.Now,
            CreatedBy = createdBy
        };
    }

    protected override async Task<bool> IsEntityUniqueAsync(AssetTransactionEntity entity, ImportResult result, int rowNumber)
    {
        if (entity.FromAssetTransactionId.HasValue)
        {
            var existing = await _transactionReps.GetByIdRawAsync(entity.FromAssetTransactionId.Value);
            if (existing == null)
            {
                result.AddError(rowNumber, "FromAssetTransactionId", $"Referenced transaction {entity.FromAssetTransactionId} not found");
                return false;
            }
        }
        return true;
    }

    protected override string GetEntityIdentifier(AssetTransactionEntity entity) => entity.AssetTransactionId.ToString();

    public override async Task<ServiceResult<ImportResult>> ImportFromFileAsync(Stream fileStream, string? createdBy = null)
    {
        var result = await base.ImportFromFileAsync(fileStream, createdBy);
        if (result.IsSuccess && result.Data != null)
            result.Message = $"Import completed: {result.Data.SuccessCount} successful, {result.Data.ErrorCount} errors. Transactions imported as PENDING and need approval.";
        return result;
    }
}