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
    /// <returns>Collection of master data groups</returns>
    Task<ServiceResult<IEnumerable<MasterDataGroupDto>>> GetAllGroupedAsync();

    /// <summary>
    /// Gets master data items by reference name
    /// </summary>
    /// <param name="referenceName">Reference name (e.g., "Position", "EmployeeStatus")</param>
    /// <returns>Collection of master data DTOs</returns>
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetByReferenceNameAsync(string referenceName);

    /// <summary>
    /// Gets a master data value by reference name and code
    /// </summary>
    /// <param name="referenceName">Reference name</param>
    /// <param name="code">Reference code</param>
    /// <returns>Master data name or null</returns>
    Task<ServiceResult<string?>> GetValueAsync(string referenceName, int code);

    /// <summary>
    /// Gets a master data code by reference name and value
    /// </summary>
    /// <param name="referenceName">Reference name</param>
    /// <param name="value">Master data name</param>
    /// <returns>Reference code or null</returns>
    Task<ServiceResult<int?>> GetCodeAsync(string referenceName, string value);

    /// <summary>
    /// Gets all transaction types for dropdown
    /// </summary>
    /// <returns>Collection of transaction type DTOs</returns>
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetTransactionTypesAsync();

    /// <summary>
    /// Gets all asset conditions for dropdown (Good, Normal, Damaged)
    /// </summary>
    /// <returns>Collection of asset condition DTOs</returns>
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetAssetConditionsAsync();

    /// <summary>
    /// Gets all employee positions for dropdown (Director, Manager, etc)
    /// </summary>
    /// <returns>Collection of position DTOs</returns>
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetEmployeePositionsAsync();

    /// <summary>
    /// Gets all employee statuses for dropdown (Permanent, Contract, etc)
    /// </summary>
    /// <returns>Collection of employee status DTOs</returns>
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetEmployeeStatusesAsync();

    /// <summary>
    /// Gets all office types for dropdown (Head Office, Branch Office)
    /// </summary>
    /// <returns>Collection of office type DTOs</returns>
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetOfficeTypesAsync();

    /// <summary>
    /// Gets all maintenance types for dropdown
    /// </summary>
    /// <returns>Collection of maintenance type DTOs</returns>
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetMaintenanceTypesAsync();

    /// <summary>
    /// Gets all asset condition purchases for dropdown (New, Second Hand)
    /// </summary>
    /// <returns>Collection of asset condition purchase DTOs</returns>
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetAssetConditionPurchasesAsync();
}