using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Asset;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Domain.Features.Reports;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Asset;

public interface IAssetService
{
    // Basic CRUD
    Task<ServiceResult<AssetDetailViewModel>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetByCategoryAsync(int categoryId);
    Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetByOfficeAsync(int officeId);
    Task<ServiceResult<AssetDetailViewModel>> CreateAsync(AssetCreateViewModel model);
    Task<ServiceResult<AssetDetailViewModel>> UpdateAsync(int id, AssetUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // Search & Grid
    Task<ServiceResult<PaginatedResult<AssetListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false, Dictionary<string, object>? filters = null);
    Task<ServiceResult<IEnumerable<AssetListViewModel>>> SearchAsync(string keyword);

    // Alerts
    Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetExpiredWarrantyAsync();
    Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetUpcomingMaintenanceAsync(int daysAhead = 30);

    // Dashboard & Tracking
    Task<ServiceResult<DashboardStatsViewModel>> GetDashboardStatsAsync();
    Task<ServiceResult<AssetTrackingViewModel>> GetAssetTrackingAsync(int assetId);
    Task<ServiceResult<AssetCurrentStatusDto>> GetCurrentStatusAsync(int assetId);
    Task<ServiceResult<IEnumerable<AssetTransactionDto>>> GetAssetTransactionHistoryAsync(int assetId);

    // Bulk operations
    Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request);
}