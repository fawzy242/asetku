using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.MasterData;

namespace Whitebird.App.Features.MasterData;

public interface IMasterDataService
{
    Task<ServiceResult<IEnumerable<MasterDataGroupDto>>> GetAllGroupedAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetByReferenceNameAsync(string referenceName);
    Task<ServiceResult<string?>> GetValueAsync(string referenceName, int code);
    Task<ServiceResult<int?>> GetCodeAsync(string referenceName, string value);
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetTransactionTypesAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetAssetConditionsAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetEmployeePositionsAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetEmployeeStatusesAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetOfficeTypesAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetMaintenanceTypesAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetAssetConditionPurchasesAsync();
}