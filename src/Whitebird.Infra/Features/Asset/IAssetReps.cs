using Whitebird.Domain.Features.Asset.Entities;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Asset;

public interface IAssetReps
{
    // Raw entity untuk CRUD operations
    Task<AssetEntity?> GetByIdRawAsync(int assetId);

    // Methods with relations (navigation properties populated via JOIN)
    Task<AssetEntity?> GetByIdWithRelationsAsync(int assetId);
    Task<IEnumerable<AssetEntity>> GetAllWithRelationsAsync();
    Task<IEnumerable<AssetEntity>> GetByCategoryWithRelationsAsync(int categoryId);
    Task<IEnumerable<AssetEntity>> GetByStatusWithRelationsAsync(string status);
    Task<IEnumerable<AssetEntity>> GetByHolderWithRelationsAsync(int employeeId);
    Task<IEnumerable<AssetEntity>> GetExpiredWarrantyWithRelationsAsync();
    Task<IEnumerable<AssetEntity>> GetUpcomingMaintenanceWithRelationsAsync(int daysAhead = 30);
    Task<bool> IsAssetCodeExistsAsync(string assetCode, int? excludeAssetId = null);
    Task<int> GetNextAssetNumberAsync();
    Task<PaginatedResult<AssetEntity>> GetPagedWithRelationsAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false, Dictionary<string, object>? filters = null);
}