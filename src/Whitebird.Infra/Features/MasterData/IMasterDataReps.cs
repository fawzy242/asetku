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

    // NOTE (P4-4): GetDistinctReferenceNamesAsync was removed — it had no callers.
    // If distinct reference names are ever needed, prefer exposing it through
    // MasterDataService, not directly from the repository.
}