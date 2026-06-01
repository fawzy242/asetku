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

    /// <summary>
    /// Gets an asset by ID with all related data
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <returns>Asset detail view or not found result</returns>
    Task<ServiceResult<AssetDetailView>> GetByIdAsync(int id);
    
    /// <summary>
    /// Gets all assets as list view
    /// </summary>
    /// <returns>Collection of asset list views</returns>
    Task<ServiceResult<IEnumerable<AssetListView>>> GetAllAsync();
    
    /// <summary>
    /// Gets assets by category ID
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>Collection of asset list views</returns>
    Task<ServiceResult<IEnumerable<AssetListView>>> GetByCategoryAsync(int categoryId);
    
    /// <summary>
    /// Gets assets by office ID
    /// </summary>
    /// <param name="officeId">Office ID</param>
    /// <returns>Collection of asset list views</returns>
    Task<ServiceResult<IEnumerable<AssetListView>>> GetByOfficeAsync(int officeId);
    
    /// <summary>
    /// Creates a new asset
    /// </summary>
    /// <param name="model">Asset creation data</param>
    /// <returns>Created asset detail view</returns>
    Task<ServiceResult<AssetDetailView>> CreateAsync(AssetCreateViewModel model);
    
    /// <summary>
    /// Updates an existing asset
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <param name="model">Asset update data</param>
    /// <returns>Updated asset detail view</returns>
    Task<ServiceResult<AssetDetailView>> UpdateAsync(int id, AssetUpdateViewModel model);
    
    /// <summary>
    /// Permanently deletes an asset
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> DeleteAsync(int id);
    
    /// <summary>
    /// Soft deletes an asset (sets IsActive = false)
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // SEARCH & GRID
    // ============================================================

    /// <summary>
    /// Gets paginated list of assets for grid display
    /// </summary>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search keyword</param>
    /// <param name="sortBy">Sort column name</param>
    /// <param name="sortDescending">True for descending sort</param>
    /// <param name="filters">Additional filters (status, categoryId, officeId, isActive)</param>
    /// <returns>Paginated result with asset list views</returns>
    Task<ServiceResult<PaginatedResult<AssetListView>>> GetGridDataAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null);
    
    /// <summary>
    /// Searches assets by keyword (minimum 2 characters)
    /// </summary>
    /// <param name="keyword">Search keyword</param>
    /// <returns>Collection of matching asset list views</returns>
    Task<ServiceResult<IEnumerable<AssetListView>>> SearchAsync(string keyword);

    // ============================================================
    // ALERTS
    // ============================================================

    /// <summary>
    /// Gets assets with expired warranty
    /// </summary>
    /// <returns>Collection of assets with expired warranty</returns>
    Task<ServiceResult<IEnumerable<AssetListView>>> GetExpiredWarrantyAsync();
    
    /// <summary>
    /// Gets assets with upcoming maintenance
    /// </summary>
    /// <param name="daysAhead">Number of days to look ahead (default 30)</param>
    /// <returns>Collection of assets needing maintenance</returns>
    Task<ServiceResult<IEnumerable<AssetListView>>> GetUpcomingMaintenanceAsync(int daysAhead = 30);

    // ============================================================
    // DASHBOARD & TRACKING
    // ============================================================

    /// <summary>
    /// Gets dashboard statistics
    /// </summary>
    /// <returns>Dashboard statistics view model</returns>
    Task<ServiceResult<DashboardStatsViewModel>> GetDashboardStatsAsync();
    
    /// <summary>
    /// Gets complete asset tracking information with timeline
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    /// <returns>Asset tracking view model</returns>
    Task<ServiceResult<AssetTrackingViewModel>> GetAssetTrackingAsync(int assetId);
    
    /// <summary>
    /// Gets current status of an asset
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    /// <returns>Current status DTO</returns>
    Task<ServiceResult<AssetCurrentStatusDto>> GetCurrentStatusAsync(int assetId);
    
    /// <summary>
    /// Gets transaction history for an asset
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    /// <returns>Collection of transaction DTOs</returns>
    Task<ServiceResult<IEnumerable<AssetTransactionDto>>> GetAssetTransactionHistoryAsync(int assetId);

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    /// <summary>
    /// Bulk activate or deactivate assets
    /// </summary>
    /// <param name="request">Bulk activation request with IDs and action</param>
    /// <returns>Number of assets affected</returns>
    Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request);
    
    // ============================================================
    // DROPDOWN
    // ============================================================

    /// <summary>
    /// Gets asset dropdown list (minimal data for selects)
    /// </summary>
    /// <returns>Collection of asset dropdown views</returns>
    Task<ServiceResult<IEnumerable<AssetDropdownView>>> GetDropdownListAsync();
}