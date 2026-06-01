using Whitebird.App.Features.Common;

namespace Whitebird.App.Features.MasterData;

/// <summary>
/// Service for looking up master data names from constants codes
/// </summary>
public interface IMasterDataLookupService
{
    /// <summary>
    /// Gets asset condition name by code
    /// </summary>
    Task<ServiceResult<string?>> GetAssetConditionNameAsync(int code);
    
    /// <summary>
    /// Gets asset condition purchase name by code
    /// </summary>
    Task<ServiceResult<string?>> GetAssetConditionPurchaseNameAsync(int code);
    
    /// <summary>
    /// Gets employee status name by code
    /// </summary>
    Task<ServiceResult<string?>> GetEmployeeStatusNameAsync(int code);
    
    /// <summary>
    /// Gets maintenance type name by code
    /// </summary>
    Task<ServiceResult<string?>> GetMaintenanceTypeNameAsync(int code);
    
    /// <summary>
    /// Gets office type name by code
    /// </summary>
    Task<ServiceResult<string?>> GetOfficeTypeNameAsync(int code);
    
    /// <summary>
    /// Gets position name by code
    /// </summary>
    Task<ServiceResult<string?>> GetPositionNameAsync(int code);
    
    /// <summary>
    /// Gets transaction type name by code
    /// </summary>
    Task<ServiceResult<string?>> GetTransactionTypeNameAsync(int code);
}