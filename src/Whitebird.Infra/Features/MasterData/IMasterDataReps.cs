using Whitebird.Domain.Features.MasterData;

namespace Whitebird.Infra.Features.MasterData;

public interface IMasterDataReps
{
    Task<MasterDataEntity?> GetByIdAsync(int id);
    Task<IEnumerable<MasterDataEntity>> GetAllAsync();
    Task<IEnumerable<MasterDataEntity>> GetByReferenceNameAsync(string referenceName);
    Task<MasterDataEntity?> GetByReferenceNameAndCodeAsync(string referenceName, int code);
    Task<bool> IsExistsAsync(string referenceName, int code);
    Task<IEnumerable<string>> GetDistinctReferenceNamesAsync();
}