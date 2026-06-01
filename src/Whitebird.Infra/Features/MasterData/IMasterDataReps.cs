using Whitebird.Domain.Features.MasterData;

namespace Whitebird.Infra.Features.MasterData;

/// <summary>
/// Repository interface for Master Data operations
/// </summary>
public interface IMasterDataReps
{
    Task<MasterDataEntity?> GetByIdAsync(int id);
    Task<IEnumerable<MasterDataEntity>> GetAllAsync();
    Task<IEnumerable<MasterDataEntity>> GetByReferenceNameAsync(string referenceName);
    Task<MasterDataEntity?> GetByReferenceNameAndCodeAsync(string referenceName, int code);
    Task<bool> IsExistsAsync(string referenceName, int code);
    Task<IEnumerable<string>> GetDistinctReferenceNamesAsync();
}