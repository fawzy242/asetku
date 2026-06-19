using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Asset;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Domain.Features.Reports;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Asset;

/// <summary>
/// Service interface for Asset business logic
/// </summary>
public interface IAssetService
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    Task<ServiceResult<AssetDetailView>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<AssetListView>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<AssetListView>>> GetByCategoryAsync(int categoryId);
    Task<ServiceResult<IEnumerable<AssetListView>>> GetByOfficeAsync(int officeId);
    Task<ServiceResult<AssetDetailView>> CreateAsync(AssetCreateViewModel model);
    Task<ServiceResult<AssetDetailView>> UpdateAsync(int id, AssetUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // SEARCH & GRID
    // ============================================================

    Task<ServiceResult<PaginatedResult<AssetListView>>> GetGridDataAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null);
    
    Task<ServiceResult<IEnumerable<AssetListView>>> SearchAsync(string keyword);

    // ============================================================
    // ALERTS
    // ============================================================

    Task<ServiceResult<IEnumerable<AssetListView>>> GetExpiredWarrantyAsync();
    Task<ServiceResult<IEnumerable<AssetListView>>> GetUpcomingMaintenanceAsync(int daysAhead = 30);

    // ============================================================
    // DASHBOARD & TRACKING
    // ============================================================

    Task<ServiceResult<DashboardStatsViewModel>> GetDashboardStatsAsync();
    Task<ServiceResult<AssetTrackingViewModel>> GetAssetTrackingAsync(int assetId);
    Task<ServiceResult<AssetCurrentStatusDto>> GetCurrentStatusAsync(int assetId);
    Task<ServiceResult<IEnumerable<AssetTransactionDto>>> GetAssetTransactionHistoryAsync(int assetId);

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request);
    
    // ============================================================
    // DROPDOWN
    // ============================================================

    Task<ServiceResult<IEnumerable<AssetDropdownView>>> GetDropdownListAsync();
    
    // ============================================================
    // NEW: AVAILABLE ASSETS FOR TRANSACTION
    // ============================================================

    /// <summary>
    /// Gets assets that are available for new transaction
    /// </summary>
    Task<ServiceResult<IEnumerable<AssetDropdownView>>> GetAvailableAssetsForTransactionAsync();
    
    /// <summary>
    /// Checks if asset is available for new transaction
    /// </summary>
    Task<ServiceResult<bool>> IsAssetAvailableForTransactionAsync(int assetId);
    
    // ============================================================
    // NEW: ASSET STATUS LISTS
    // ============================================================

    /// <summary>
    /// Gets damaged assets
    /// </summary>
    Task<ServiceResult<IEnumerable<AssetListView>>> GetDamagedAssetsAsync();
    
    /// <summary>
    /// Gets inactive assets
    /// </summary>
    Task<ServiceResult<IEnumerable<AssetListView>>> GetInactiveAssetsAsync();
}