using System.Data;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Common.Import;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.AssetTransaction;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Employee;
using Whitebird.Infra.Features.Office;

namespace Whitebird.App.Features.AssetTransaction;

public class TransactionImportService : ImportServiceBase<TransactionImportDto, AssetTransactionEntity>
{
    private readonly IAssetTransactionReps _transactionReps;
    private readonly IAssetReps _assetReps;
    private readonly IEmployeeReps _employeeReps;
    private readonly IOfficeReps _officeReps;
    private readonly IMasterDataService _masterDataService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    private readonly Dictionary<string, string> _templateColumns = new()
    {
        { "AssetCode", "Required. Asset code must exist in system" },
        { "TransactionType", "Required. Values: HANDOVER, TRANSFER, LOAN, RETURN, LOAN_RETURN, MAINTENANCE, POST_MAINTENANCE, DISPOSAL" },
        { "FromEmployeeCode", "Optional. Employee code of the sender (for TRANSFER, RETURN, LOAN_RETURN)" },
        { "ToEmployeeCode", "Optional. Employee code of the receiver (for HANDOVER, TRANSFER, LOAN)" },
        { "ToOfficeName", "Optional. Office name for location change" },
        { "TransactionDate", "Required. Format: YYYY-MM-DD HH:MM:SS" },
        { "ExpectedReturnDate", "Optional. Format: YYYY-MM-DD (required for LOAN)" },
        { "ActualReturnDate", "Optional. Format: YYYY-MM-DD" },
        { "Notes", "Optional. Additional notes" },
        { "ConditionBefore", "Optional. Values: Good, Normal, Damaged" },
        { "ConditionAfter", "Optional. Values: Good, Normal, Damaged" },
        { "MaintenanceType", "Optional. Values: PREVENTIVE MAINTENANCE, CORRECTIVE MAINTENANCE, EMERGENCY REPAIR, etc" },
        { "MaintenanceCost", "Optional. Numeric value" },
        { "FromAssetTransactionId", "Optional. For pairing LOAN_RETURN or POST_MAINTENANCE" }
    };

    public TransactionImportService(
        IGenericRepository<AssetTransactionEntity> repository,
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
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    protected override Dictionary<string, string> GetTemplateColumns() => _templateColumns;

    protected override async Task InitializeCachesAsync(Dictionary<string, object> cache)
    {
        var assetCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var allAssets = await _assetReps.GetAllListViewAsync();
        foreach (var a in allAssets)
        {
            assetCache[a.AssetCode] = a.AssetId;
        }
        cache["Assets"] = assetCache;

        var employeeDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var allEmployees = await _employeeReps.GetActiveOnlyListViewAsync();
        foreach (var e in allEmployees)
        {
            employeeDict[e.EmployeeCode] = e.EmployeeId;
        }
        cache["Employees"] = employeeDict;

        var officeDict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var allOffices = await _officeReps.GetAllListViewAsync();
        foreach (var o in allOffices)
        {
            officeDict[o.OfficeName] = o.OfficeId;
        }
        cache["Offices"] = officeDict;

        var transactionTypeCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var transactionTypes = await _masterDataService.GetTransactionTypesAsync();
        if (transactionTypes.IsSuccess && transactionTypes.Data != null)
        {
            foreach (var t in transactionTypes.Data)
                transactionTypeCache[t.Name] = t.Code;
        }
        cache["TransactionTypes"] = transactionTypeCache;

        var conditionCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var conditions = await _masterDataService.GetAssetConditionsAsync();
        if (conditions.IsSuccess && conditions.Data != null)
        {
            foreach (var c in conditions.Data)
                conditionCache[c.Name] = c.Code;
        }
        cache["Conditions"] = conditionCache;

        var maintenanceTypeCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var maintenanceTypes = await _masterDataService.GetMaintenanceTypesAsync();
        if (maintenanceTypes.IsSuccess && maintenanceTypes.Data != null)
        {
            foreach (var m in maintenanceTypes.Data)
                maintenanceTypeCache[m.Name] = m.Code;
        }
        cache["MaintenanceTypes"] = maintenanceTypeCache;
    }

    protected override async Task<AssetTransactionEntity?> ProcessRowAsync(
        DataRow row,
        int rowNumber,
        ImportResult result,
        Dictionary<string, object> cache,
        string createdBy)
    {
        var assetCache = cache["Assets"] as Dictionary<string, int>;
        var employeeDict = cache["Employees"] as Dictionary<string, int>;
        var officeDict = cache["Offices"] as Dictionary<string, int>;
        var transactionTypeCache = cache["TransactionTypes"] as Dictionary<string, int>;
        var conditionCache = cache["Conditions"] as Dictionary<string, int>;
        var maintenanceTypeCache = cache["MaintenanceTypes"] as Dictionary<string, int>;

        var assetCode = ExcelDataReader.GetString(row, "AssetCode");
        var transactionTypeName = ExcelDataReader.GetString(row, "TransactionType");
        var fromEmployeeCode = ExcelDataReader.GetString(row, "FromEmployeeCode");
        var toEmployeeCode = ExcelDataReader.GetString(row, "ToEmployeeCode");
        var toOfficeName = ExcelDataReader.GetString(row, "ToOfficeName");
        var notes = ExcelDataReader.GetString(row, "Notes");
        var conditionBeforeName = ExcelDataReader.GetString(row, "ConditionBefore");
        var conditionAfterName = ExcelDataReader.GetString(row, "ConditionAfter");
        var maintenanceTypeName = ExcelDataReader.GetString(row, "MaintenanceType");
        var transactionDate = ExcelDataReader.GetDateTime(row, "TransactionDate");
        var expectedReturnDate = ExcelDataReader.GetNullableDateTime(row, "ExpectedReturnDate");
        var actualReturnDate = ExcelDataReader.GetNullableDateTime(row, "ActualReturnDate");
        var maintenanceCost = ExcelDataReader.GetNullableDecimal(row, "MaintenanceCost");
        var fromAssetTransactionId = ExcelDataReader.GetNullableInt(row, "FromAssetTransactionId");

        if (string.IsNullOrWhiteSpace(assetCode))
        {
            result.AddError(rowNumber, "AssetCode", "AssetCode is required");
            return null;
        }

        if (string.IsNullOrWhiteSpace(transactionTypeName))
        {
            result.AddError(rowNumber, "TransactionType", "TransactionType is required");
            return null;
        }

        if (transactionDate == DateTime.MinValue)
        {
            result.AddError(rowNumber, "TransactionDate", "TransactionDate is required");
            return null;
        }

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

        int? fromEmployeeId = null;
        if (!string.IsNullOrWhiteSpace(fromEmployeeCode) && employeeDict != null)
        {
            if (!employeeDict.TryGetValue(fromEmployeeCode, out int empId))
                result.AddError(rowNumber, "FromEmployeeCode", $"Employee '{fromEmployeeCode}' not found", fromEmployeeCode);
            else
                fromEmployeeId = empId;
        }

        int? toEmployeeId = null;
        if (!string.IsNullOrWhiteSpace(toEmployeeCode) && employeeDict != null)
        {
            if (!employeeDict.TryGetValue(toEmployeeCode, out int empId))
                result.AddError(rowNumber, "ToEmployeeCode", $"Employee '{toEmployeeCode}' not found", toEmployeeCode);
            else
                toEmployeeId = empId;
        }

        int? officeId = null;
        if (!string.IsNullOrWhiteSpace(toOfficeName) && officeDict != null)
        {
            if (!officeDict.TryGetValue(toOfficeName, out int offId))
                result.AddError(rowNumber, "ToOfficeName", $"Office '{toOfficeName}' not found", toOfficeName);
            else
                officeId = offId;
        }

        int? conditionBeforeId = null;
        if (!string.IsNullOrWhiteSpace(conditionBeforeName) && conditionCache != null)
        {
            if (!conditionCache.TryGetValue(conditionBeforeName, out int condId))
                result.AddError(rowNumber, "ConditionBefore", $"Invalid ConditionBefore '{conditionBeforeName}'. Valid: Good, Normal, Damaged", conditionBeforeName);
            else
                conditionBeforeId = condId;
        }

        int? conditionAfterId = null;
        if (!string.IsNullOrWhiteSpace(conditionAfterName) && conditionCache != null)
        {
            if (!conditionCache.TryGetValue(conditionAfterName, out int condId))
                result.AddError(rowNumber, "ConditionAfter", $"Invalid ConditionAfter '{conditionAfterName}'. Valid: Good, Normal, Damaged", conditionAfterName);
            else
                conditionAfterId = condId;
        }

        int? maintenanceTypeId = null;
        if (!string.IsNullOrWhiteSpace(maintenanceTypeName) && maintenanceTypeCache != null)
        {
            if (!maintenanceTypeCache.TryGetValue(maintenanceTypeName, out int maintId))
                result.AddError(rowNumber, "MaintenanceType", $"Invalid MaintenanceType '{maintenanceTypeName}'", maintenanceTypeName);
            else
                maintenanceTypeId = maintId;
        }

        var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(assetId);

        if (transactionTypeId == 1) // HANDOVER
        {
            if (string.IsNullOrWhiteSpace(toEmployeeCode))
            {
                result.AddError(rowNumber, "ToEmployeeCode", "HANDOVER requires ToEmployeeCode");
                return null;
            }
            if (activeTransaction != null)
            {
                result.AddError(rowNumber, "AssetCode", $"Asset '{assetCode}' is currently in an active transaction");
                return null;
            }
        }

        if (transactionTypeId == 2) // TRANSFER
        {
            if (string.IsNullOrWhiteSpace(fromEmployeeCode) || string.IsNullOrWhiteSpace(toEmployeeCode))
            {
                result.AddError(rowNumber, "FromEmployeeCode/ToEmployeeCode", "TRANSFER requires both FromEmployeeCode and ToEmployeeCode");
                return null;
            }
        }

        if (transactionTypeId == 3) // LOAN
        {
            if (string.IsNullOrWhiteSpace(toEmployeeCode))
            {
                result.AddError(rowNumber, "ToEmployeeCode", "LOAN requires ToEmployeeCode");
                return null;
            }
            if (!expectedReturnDate.HasValue)
            {
                result.AddError(rowNumber, "ExpectedReturnDate", "LOAN requires ExpectedReturnDate");
                return null;
            }
            if (expectedReturnDate <= transactionDate)
            {
                result.AddError(rowNumber, "ExpectedReturnDate", "ExpectedReturnDate must be after TransactionDate");
                return null;
            }
        }

        if (transactionTypeId == 4) // RETURN
        {
            if (string.IsNullOrWhiteSpace(fromEmployeeCode))
            {
                result.AddError(rowNumber, "FromEmployeeCode", "RETURN requires FromEmployeeCode");
                return null;
            }
        }

        if (result.Errors.Any(e => e.RowNumber == rowNumber))
            return null;

        return new AssetTransactionEntity
        {
            AssetId = assetId,
            TransactionType = transactionTypeId,
            FromEmployeeId = fromEmployeeId,
            ToEmployeeId = toEmployeeId,
            ToLocationId = officeId,
            TransactionDate = transactionDate,
            ExpectedReturnDate = expectedReturnDate,
            ActualReturnDate = actualReturnDate,
            Notes = notes,
            ConditionBefore = conditionBeforeId,
            ConditionAfter = conditionAfterId,
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
        // Transactions don't have unique constraint per se, but we check for duplicate reference
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

    public override async Task<ServiceResult<ImportResult>> ImportFromExcelAsync(Stream fileStream, string? createdBy = null)
    {
        var result = await base.ImportFromExcelAsync(fileStream, createdBy);
        if (result.IsSuccess && result.Data != null)
        {
            result.Message = $"Import completed: {result.Data.SuccessCount} successful, {result.Data.ErrorCount} errors. Transactions imported as PENDING and need approval.";
        }
        return result;
    }
}