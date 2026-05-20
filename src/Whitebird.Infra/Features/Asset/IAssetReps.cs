using Whitebird.Domain.Features.Asset;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Asset;

public interface IAssetReps
{
    Task<AssetEntity?> GetByIdRawAsync(int assetId);

    Task<AssetEntity?> GetByIdWithRelationsAsync(int assetId);
    Task<IEnumerable<AssetEntity>> GetAllWithRelationsAsync();
    Task<IEnumerable<AssetEntity>> GetByCategoryWithRelationsAsync(int categoryId);
    Task<IEnumerable<AssetEntity>> GetByOfficeWithRelationsAsync(int officeId);
    Task<IEnumerable<AssetEntity>> GetByHolderWithRelationsAsync(int employeeId);
    Task<IEnumerable<AssetEntity>> GetExpiredWarrantyWithRelationsAsync();
    Task<IEnumerable<AssetEntity>> GetUpcomingMaintenanceWithRelationsAsync(int daysAhead = 30);

    Task<bool> IsAssetCodeExistsAsync(string assetCode, int? excludeAssetId = null);

    Task<PaginatedResult<AssetEntity>> GetPagedWithRelationsAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false, Dictionary<string, object>? filters = null);

    Task<int> GetTotalAssetsCountAsync();
    Task<int> GetAvailableAssetsCountAsync();
    Task<int> GetAssignedAssetsCountAsync();
    Task<int> GetAssetsOnLoanCountAsync();
    Task<int> GetAssetsInMaintenanceCountAsync();
    Task<int> GetExpiredWarrantyCountAsync();
    Task<int> GetUpcomingMaintenanceCountAsync(int daysAhead = 30);
    Task<decimal> GetTotalAssetValueAsync();
}