using Whitebird.Domain.Features.Asset;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Asset;

/// <summary>
/// Repository interface for Asset operations
/// </summary>
public interface IAssetReps
{
    // ============================================================
    // RAW ENTITY METHODS (For internal Service use only - NOT for API)
    // ============================================================
    
    /// <summary>
    /// Gets asset entity by ID (RAW - returns Entity, for internal use only)
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    /// <returns>Asset entity or null if not found</returns>
    Task<AssetEntity?> GetByIdRawAsync(int assetId);
    
    /// <summary>
    /// Checks if asset code already exists
    /// </summary>
    /// <param name="assetCode">Asset code to check</param>
    /// <param name="excludeAssetId">Optional asset ID to exclude from check (for updates)</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> IsAssetCodeExistsAsync(string assetCode, int? excludeAssetId = null);
    
    // ============================================================
    // DETAIL VIEW METHODS (For API responses)
    // ============================================================
    
    /// <summary>
    /// Gets asset detail view by ID (includes all JOIN data)
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    /// <returns>Asset detail view or null if not found</returns>
    Task<AssetDetailView?> GetDetailByIdAsync(int assetId);
    
    /// <summary>
    /// Gets all assets as list view (for grid/list display)
    /// </summary>
    /// <returns>Collection of asset list views</returns>
    Task<IEnumerable<AssetListView>> GetAllListViewAsync();
    
    /// <summary>
    /// Gets assets by category ID as list view
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>Collection of asset list views</returns>
    Task<IEnumerable<AssetListView>> GetByCategoryListViewAsync(int categoryId);
    
    /// <summary>
    /// Gets assets by office ID as list view
    /// </summary>
    /// <param name="officeId">Office ID</param>
    /// <returns>Collection of asset list views</returns>
    Task<IEnumerable<AssetListView>> GetByOfficeListViewAsync(int officeId);
    
    /// <summary>
    /// Gets assets by current holder employee ID as list view
    /// </summary>
    /// <param name="employeeId">Employee ID (current holder)</param>
    /// <returns>Collection of asset list views</returns>
    Task<IEnumerable<AssetListView>> GetByHolderListViewAsync(int employeeId);
    
    /// <summary>
    /// Gets assets with expired warranty as list view
    /// </summary>
    /// <returns>Collection of asset list views with expired warranty</returns>
    Task<IEnumerable<AssetListView>> GetExpiredWarrantyListViewAsync();
    
    /// <summary>
    /// Gets assets with upcoming maintenance as list view
    /// </summary>
    /// <param name="daysAhead">Number of days to look ahead (default 30)</param>
    /// <returns>Collection of asset list views</returns>
    Task<IEnumerable<AssetListView>> GetUpcomingMaintenanceListViewAsync(int daysAhead = 30);
    
    /// <summary>
    /// Gets assets filtered by status as list view
    /// </summary>
    /// <param name="status">Status filter (Available, Assigned, On Loan, In Maintenance, Active, Inactive)</param>
    /// <returns>Collection of asset list views</returns>
    Task<IEnumerable<AssetListView>> GetByStatusListViewAsync(string status);
    
    /// <summary>
    /// Searches assets by keyword
    /// </summary>
    /// <param name="keyword">Search keyword (min 2 chars)</param>
    /// <param name="limit">Maximum results to return (default 10)</param>
    /// <returns>Collection of matching asset list views</returns>
    Task<IEnumerable<AssetListView>> SearchAssetsAsync(string keyword, int limit = 10);
    
    // ============================================================
    // COUNT METHODS
    // ============================================================
    
    /// <summary>
    /// Gets total number of assets
    /// </summary>
    /// <returns>Total asset count</returns>
    Task<int> GetTotalAssetsCountAsync();
    
    /// <summary>
    /// Gets number of available assets (not assigned to anyone)
    /// </summary>
    /// <returns>Available asset count</returns>
    Task<int> GetAvailableAssetsCountAsync();
    
    /// <summary>
    /// Gets number of assigned assets (handover or transfer)
    /// </summary>
    /// <returns>Assigned asset count</returns>
    Task<int> GetAssignedAssetsCountAsync();
    
    /// <summary>
    /// Gets number of assets on loan
    /// </summary>
    /// <returns>On loan asset count</returns>
    Task<int> GetAssetsOnLoanCountAsync();
    
    /// <summary>
    /// Gets number of assets in maintenance
    /// </summary>
    /// <returns>In maintenance asset count</returns>
    Task<int> GetAssetsInMaintenanceCountAsync();
    
    /// <summary>
    /// Gets number of assets with expired warranty
    /// </summary>
    /// <returns>Expired warranty count</returns>
    Task<int> GetExpiredWarrantyCountAsync();
    
    /// <summary>
    /// Gets number of assets with upcoming maintenance
    /// </summary>
    /// <param name="daysAhead">Number of days to look ahead</param>
    /// <returns>Upcoming maintenance count</returns>
    Task<int> GetUpcomingMaintenanceCountAsync(int daysAhead = 30);
    
    /// <summary>
    /// Gets total value of all assets (sum of purchase price)
    /// </summary>
    /// <returns>Total asset value</returns>
    Task<decimal> GetTotalAssetValueAsync();

    // ============================================================
    // PAGINATION METHODS
    // ============================================================
    
    /// <summary>
    /// Gets paged list of assets with filtering, sorting, and search
    /// </summary>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search keyword</param>
    /// <param name="sortBy">Sort column name</param>
    /// <param name="sortDescending">True for descending sort</param>
    /// <param name="filters">Additional filters (status, categoryId, officeId, isActive, etc)</param>
    /// <returns>Paginated result with asset list views</returns>
    Task<PaginatedResult<AssetListView>> GetPagedListAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null);
    
    /// <summary>
    /// Gets asset dropdown list (minimal data for selects)
    /// </summary>
    /// <returns>Collection of asset dropdown views</returns>
    Task<IEnumerable<AssetDropdownView>> GetDropdownListAsync();
}