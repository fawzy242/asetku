using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;

namespace Whitebird.App.Features.MasterData;

/// <summary>
/// Service implementation for looking up master data names from constants codes
/// </summary>
public class MasterDataLookupService : BaseService, IMasterDataLookupService
{
    private readonly IMasterDataService _masterDataService;

    public MasterDataLookupService(
        IMasterDataService masterDataService,
        ILogger<MasterDataLookupService> logger) : base(logger)
    {
        _masterDataService = masterDataService;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string?>> GetAssetConditionNameAsync(int code)
    {
        return await _masterDataService.GetValueAsync("AssetCondition", code);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string?>> GetAssetConditionPurchaseNameAsync(int code)
    {
        return await _masterDataService.GetValueAsync("AssetConditionPurchase", code);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string?>> GetEmployeeStatusNameAsync(int code)
    {
        return await _masterDataService.GetValueAsync("EmployeeStatus", code);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string?>> GetMaintenanceTypeNameAsync(int code)
    {
        return await _masterDataService.GetValueAsync("MaintenanceType", code);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string?>> GetOfficeTypeNameAsync(int code)
    {
        return await _masterDataService.GetValueAsync("OfficeType", code);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string?>> GetPositionNameAsync(int code)
    {
        return await _masterDataService.GetValueAsync("Position", code);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string?>> GetTransactionTypeNameAsync(int code)
    {
        return await _masterDataService.GetValueAsync("TransactionType", code);
    }
}