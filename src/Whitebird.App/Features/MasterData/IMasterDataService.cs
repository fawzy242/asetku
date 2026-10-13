using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.MasterData;

namespace Whitebird.App.Features.MasterData;

/// <summary>
/// Service interface for Master Data business logic
/// </summary>
public interface IMasterDataService
{
    /// <summary>
    /// Gets all master data grouped by ReferenceName
    /// </summary>
    Task<ServiceResult<IEnumerable<MasterDataGroupDto>>> GetAllGroupedAsync();

    /// <summary>
    /// Gets master data items by reference name
    /// </summary>
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetByReferenceNameAsync(string referenceName);

    /// <summary>
    /// Gets a master data value by reference name and code
    /// </summary>
    Task<ServiceResult<string?>> GetValueAsync(string referenceName, int code);

    /// <summary>
    /// Gets all transaction types for dropdown
    /// </summary>
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetTransactionTypesAsync();

    /// <summary>
    /// Gets all asset conditions for dropdown (Good, Normal, Damaged)
    /// </summary>
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetAssetConditionsAsync();

    /// <summary>
    /// Gets all employee positions for dropdown (Director, Manager, etc)
    /// </summary>
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetEmployeePositionsAsync();

    /// <summary>
    /// Gets all employee statuses for dropdown (Permanent, Contract, etc)
    /// </summary>
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetEmployeeStatusesAsync();

    /// <summary>
    /// Gets all office types for dropdown (Head Office, Branch Office)
    /// </summary>
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetOfficeTypesAsync();

    /// <summary>
    /// Gets all maintenance types for dropdown
    /// </summary>
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetMaintenanceTypesAsync();

    /// <summary>
    /// Gets all asset condition purchases for dropdown (New, Second Hand)
    /// </summary>
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetAssetConditionPurchasesAsync();
}