using System.Data;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Common.Import;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.AssetTransaction;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Employee;
using Whitebird.Infra.Features.Office;

namespace Whitebird.App.Features.AssetTransaction;

public class TransactionImportService : BaseService, ITransactionImportService
{
    private readonly IGenericRepository<AssetTransactionEntity> _repository;
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
        { "MaintenanceType", "Optional. Values: PREVENTIVE_MAINTENANCE, CORRECTIVE_MAINTENANCE, EMERGENCY_REPAIR, etc" },
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
        ILogger<TransactionImportService> logger) : base(logger)
    {
        _repository = repository;
        _transactionReps = transactionReps;
        _assetReps = assetReps;
        _employeeReps = employeeReps;
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
            var transactionsToInsert = new List<AssetTransactionEntity>();
            var createdByUser = createdBy ?? _currentUserService.GetDisplayName();

            var dataTable = await ExcelHelper.ReadExcelToDataTableAsync(fileStream, true);
            result.TotalRows = dataTable.Rows.Count;

            var assetCache = new Dictionary<string, int>();
            var employeeCache = new Dictionary<string, int>();
            var officeCache = new Dictionary<string, int>();
            var transactionTypeCache = new Dictionary<string, int>();
            var conditionCache = new Dictionary<string, int>();
            var maintenanceTypeCache = new Dictionary<string, int>();

            var transactionTypes = await _masterDataService.GetTransactionTypesAsync();
            if (transactionTypes.IsSuccess && transactionTypes.Data != null)
            {
                foreach (var t in transactionTypes.Data)
                    transactionTypeCache[t.Name.ToUpperInvariant()] = t.Code;
            }

            var conditions = await _masterDataService.GetAssetConditionsAsync();
            if (conditions.IsSuccess && conditions.Data != null)
            {
                foreach (var c in conditions.Data)
                    conditionCache[c.Name.ToUpperInvariant()] = c.Code;
            }

            var maintenanceTypes = await _masterDataService.GetMaintenanceTypesAsync();
            if (maintenanceTypes.IsSuccess && maintenanceTypes.Data != null)
            {
                foreach (var m in maintenanceTypes.Data)
                    maintenanceTypeCache[m.Name.ToUpperInvariant()] = m.Code;
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = dataTable.Rows[i];
                var rowNumber = i + 2;
                var dto = new TransactionImportDto();

                try
                {
                    dto.AssetCode = GetString(row, "AssetCode");
                    dto.TransactionTypeName = GetString(row, "TransactionType");
                    dto.FromEmployeeCode = GetString(row, "FromEmployeeCode");
                    dto.ToEmployeeCode = GetString(row, "ToEmployeeCode");
                    dto.ToOfficeName = GetString(row, "ToOfficeName");
                    dto.Notes = GetString(row, "Notes");
                    dto.ConditionBeforeName = GetString(row, "ConditionBefore");
                    dto.ConditionAfterName = GetString(row, "ConditionAfter");
                    dto.MaintenanceTypeName = GetString(row, "MaintenanceType");
                    dto.MaintenanceCost = GetNullableDecimal(row, "MaintenanceCost");
                    dto.FromAssetTransactionId = GetNullableInt(row, "FromAssetTransactionId");

                    dto.TransactionDate = GetDateTime(row, "TransactionDate");
                    dto.ExpectedReturnDate = GetNullableDateTime(row, "ExpectedReturnDate");
                    dto.ActualReturnDate = GetNullableDateTime(row, "ActualReturnDate");

                    if (string.IsNullOrWhiteSpace(dto.AssetCode))
                    {
                        result.AddError(rowNumber, "AssetCode", "AssetCode is required");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(dto.TransactionTypeName))
                    {
                        result.AddError(rowNumber, "TransactionType", "TransactionType is required");
                        continue;
                    }

                    if (dto.TransactionDate == DateTime.MinValue)
                    {
                        result.AddError(rowNumber, "TransactionDate", "TransactionDate is required");
                        continue;
                    }

                    int? assetId = null;
                    if (!assetCache.ContainsKey(dto.AssetCode.ToUpperInvariant()))
                    {
                        var allAssets = await _assetReps.GetAllWithRelationsAsync();
                        var currentAsset = allAssets.FirstOrDefault(a => a.AssetCode.Equals(dto.AssetCode, StringComparison.OrdinalIgnoreCase));
                        if (currentAsset != null)
                            assetCache[dto.AssetCode.ToUpperInvariant()] = currentAsset.AssetId;
                        else
                            result.AddError(rowNumber, "AssetCode", $"Asset '{dto.AssetCode}' not found", dto.AssetCode);
                    }

                    if (assetCache.ContainsKey(dto.AssetCode.ToUpperInvariant()))
                        assetId = assetCache[dto.AssetCode.ToUpperInvariant()];

                    if (!assetId.HasValue)
                        continue;

                    int? transactionTypeId = null;
                    var typeKey = dto.TransactionTypeName.ToUpperInvariant();
                    if (transactionTypeCache.ContainsKey(typeKey))
                        transactionTypeId = transactionTypeCache[typeKey];
                    else
                        result.AddError(rowNumber, "TransactionType", $"Invalid TransactionType '{dto.TransactionTypeName}'. Valid values: HANDOVER, TRANSFER, LOAN, RETURN, LOAN_RETURN, MAINTENANCE, POST_MAINTENANCE, DISPOSAL", dto.TransactionTypeName);

                    if (!transactionTypeId.HasValue)
                        continue;

                    int? fromEmployeeId = null;
                    if (!string.IsNullOrWhiteSpace(dto.FromEmployeeCode))
                    {
                        if (!employeeCache.ContainsKey(dto.FromEmployeeCode.ToUpperInvariant()))
                        {
                            var employees = await _employeeReps.GetAllAsync();
                            var employee = employees.FirstOrDefault(e => e.EmployeeCode.Equals(dto.FromEmployeeCode, StringComparison.OrdinalIgnoreCase));
                            if (employee != null)
                                employeeCache[dto.FromEmployeeCode.ToUpperInvariant()] = employee.EmployeeId;
                            else
                                result.AddError(rowNumber, "FromEmployeeCode", $"Employee '{dto.FromEmployeeCode}' not found", dto.FromEmployeeCode);
                        }

                        if (employeeCache.ContainsKey(dto.FromEmployeeCode.ToUpperInvariant()))
                            fromEmployeeId = employeeCache[dto.FromEmployeeCode.ToUpperInvariant()];
                    }

                    int? toEmployeeId = null;
                    if (!string.IsNullOrWhiteSpace(dto.ToEmployeeCode))
                    {
                        if (!employeeCache.ContainsKey(dto.ToEmployeeCode.ToUpperInvariant()))
                        {
                            var employees = await _employeeReps.GetAllAsync();
                            var employee = employees.FirstOrDefault(e => e.EmployeeCode.Equals(dto.ToEmployeeCode, StringComparison.OrdinalIgnoreCase));
                            if (employee != null)
                                employeeCache[dto.ToEmployeeCode.ToUpperInvariant()] = employee.EmployeeId;
                            else
                                result.AddError(rowNumber, "ToEmployeeCode", $"Employee '{dto.ToEmployeeCode}' not found", dto.ToEmployeeCode);
                        }

                        if (employeeCache.ContainsKey(dto.ToEmployeeCode.ToUpperInvariant()))
                            toEmployeeId = employeeCache[dto.ToEmployeeCode.ToUpperInvariant()];
                    }

                    int? officeId = null;
                    if (!string.IsNullOrWhiteSpace(dto.ToOfficeName))
                    {
                        if (!officeCache.ContainsKey(dto.ToOfficeName.ToUpperInvariant()))
                        {
                            var offices = await _officeReps.GetAllAsync();
                            var office = offices.FirstOrDefault(o => o.OfficeName.Equals(dto.ToOfficeName, StringComparison.OrdinalIgnoreCase));
                            if (office != null)
                                officeCache[dto.ToOfficeName.ToUpperInvariant()] = office.OfficeId;
                            else
                                result.AddError(rowNumber, "ToOfficeName", $"Office '{dto.ToOfficeName}' not found", dto.ToOfficeName);
                        }

                        if (officeCache.ContainsKey(dto.ToOfficeName.ToUpperInvariant()))
                            officeId = officeCache[dto.ToOfficeName.ToUpperInvariant()];
                    }

                    int? conditionBeforeId = null;
                    if (!string.IsNullOrWhiteSpace(dto.ConditionBeforeName))
                    {
                        var key = dto.ConditionBeforeName.ToUpperInvariant();
                        if (conditionCache.ContainsKey(key))
                            conditionBeforeId = conditionCache[key];
                        else
                            result.AddError(rowNumber, "ConditionBefore", $"Invalid ConditionBefore '{dto.ConditionBeforeName}'. Valid: Good, Normal, Damaged", dto.ConditionBeforeName);
                    }

                    int? conditionAfterId = null;
                    if (!string.IsNullOrWhiteSpace(dto.ConditionAfterName))
                    {
                        var key = dto.ConditionAfterName.ToUpperInvariant();
                        if (conditionCache.ContainsKey(key))
                            conditionAfterId = conditionCache[key];
                        else
                            result.AddError(rowNumber, "ConditionAfter", $"Invalid ConditionAfter '{dto.ConditionAfterName}'. Valid: Good, Normal, Damaged", dto.ConditionAfterName);
                    }

                    int? maintenanceTypeId = null;
                    if (!string.IsNullOrWhiteSpace(dto.MaintenanceTypeName))
                    {
                        var key = dto.MaintenanceTypeName.ToUpperInvariant();
                        if (maintenanceTypeCache.ContainsKey(key))
                            maintenanceTypeId = maintenanceTypeCache[key];
                        else
                            result.AddError(rowNumber, "MaintenanceType", $"Invalid MaintenanceType '{dto.MaintenanceTypeName}'", dto.MaintenanceTypeName);
                    }

                    if (result.Errors.Any(e => e.RowNumber == rowNumber))
                        continue;

                    var asset = await _assetReps.GetByIdRawAsync(assetId.Value);
                    var activeTransaction = await _transactionReps.GetActiveTransactionByAssetIdAsync(asset.AssetId);

                    if (transactionTypeId == 1)
                    {
                        if (string.IsNullOrWhiteSpace(dto.ToEmployeeCode))
                        {
                            result.AddError(rowNumber, "ToEmployeeCode", "HANDOVER requires ToEmployeeCode");
                            continue;
                        }
                        if (activeTransaction != null)
                        {
                            result.AddError(rowNumber, "AssetCode", $"Asset '{asset.AssetCode}' is currently in an active transaction");
                            continue;
                        }
                    }

                    if (transactionTypeId == 2)
                    {
                        if (string.IsNullOrWhiteSpace(dto.FromEmployeeCode) || string.IsNullOrWhiteSpace(dto.ToEmployeeCode))
                        {
                            result.AddError(rowNumber, "FromEmployeeCode/ToEmployeeCode", "TRANSFER requires both FromEmployeeCode and ToEmployeeCode");
                            continue;
                        }
                    }

                    if (transactionTypeId == 3)
                    {
                        if (string.IsNullOrWhiteSpace(dto.ToEmployeeCode))
                        {
                            result.AddError(rowNumber, "ToEmployeeCode", "LOAN requires ToEmployeeCode");
                            continue;
                        }
                        if (!dto.ExpectedReturnDate.HasValue)
                        {
                            result.AddError(rowNumber, "ExpectedReturnDate", "LOAN requires ExpectedReturnDate");
                            continue;
                        }
                        if (dto.ExpectedReturnDate <= dto.TransactionDate)
                        {
                            result.AddError(rowNumber, "ExpectedReturnDate", "ExpectedReturnDate must be after TransactionDate");
                            continue;
                        }
                    }

                    if (transactionTypeId == 4)
                    {
                        if (string.IsNullOrWhiteSpace(dto.FromEmployeeCode))
                        {
                            result.AddError(rowNumber, "FromEmployeeCode", "RETURN requires FromEmployeeCode");
                            continue;
                        }
                    }

                    var entity = new AssetTransactionEntity
                    {
                        AssetId = assetId.Value,
                        TransactionType = transactionTypeId.Value,
                        FromEmployeeId = fromEmployeeId,
                        ToEmployeeId = toEmployeeId,
                        ToLocationId = officeId,
                        TransactionDate = dto.TransactionDate,
                        ExpectedReturnDate = dto.ExpectedReturnDate,
                        ActualReturnDate = dto.ActualReturnDate,
                        Notes = dto.Notes,
                        ConditionBefore = conditionBeforeId,
                        ConditionAfter = conditionAfterId,
                        MaintenanceType = maintenanceTypeId,
                        MaintenanceCost = dto.MaintenanceCost,
                        FromAssetTransactionId = dto.FromAssetTransactionId,
                        Approved = null,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdByUser
                    };

                    transactionsToInsert.Add(entity);
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.AddError(rowNumber, "General", $"Error processing row: {ex.Message}");
                    _logger.LogError(ex, "Error processing transaction import row {RowNumber}", rowNumber);
                }
            }

            if (transactionsToInsert.Any())
            {
                foreach (var transaction in transactionsToInsert)
                {
                    var id = await _repository.InsertAsync(transaction);
                    
                    await _activityLogService.LogCreateAsync(
                        "AssetTransaction",
                        Convert.ToInt32(id),
                        $"Transaction imported (PENDING APPROVAL): Type '{transaction.TransactionType}' for Asset ID {transaction.AssetId}",
                        createdByUser);
                }
            }

            return ServiceResult<ImportResult>.Success(result,
                $"Import completed: {result.SuccessCount} successful, {result.ErrorCount} errors. Transactions imported as PENDING and need approval.");
        }, "import transactions", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("AssetTransaction", 0, "Import Transactions", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<byte[]>> GenerateTemplateAsync()
    {
        return await ExecuteSafelyAsync(() =>
        {
            var template = ExcelHelper.GenerateTemplate("Transactions", _templateColumns,
                "Transaction Import Template - Fill in the data below. Transactions will be imported as PENDING and need manual approval.");
            return Task.FromResult(ServiceResult<byte[]>.Success(template));
        }, "generate transaction import template");
    }

    public async Task<ServiceResult<ImportResult>> ValidateOnlyAsync(Stream fileStream)
    {
        var result = new ImportResult();
        return ServiceResult<ImportResult>.Success(result);
    }

    #region Private Helpers

    private string? GetString(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return null;
        var value = row[columnName]?.ToString();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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

    private DateTime GetDateTime(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return DateTime.MinValue;
        var value = row[columnName]?.ToString();
        if (string.IsNullOrWhiteSpace(value))
            return DateTime.MinValue;
        return DateTime.TryParse(value, out var result) ? result : DateTime.MinValue;
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

    #endregion
}