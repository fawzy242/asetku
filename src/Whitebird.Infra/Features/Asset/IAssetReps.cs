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
    Task<AssetEntity?> GetByIdRawAsync(int assetId);
    
    /// <summary>
    /// Checks if asset code already exists
    /// </summary>
    Task<bool> IsAssetCodeExistsAsync(string assetCode, int? excludeAssetId = null);
    
    // ============================================================
    // DETAIL VIEW METHODS (For API responses)
    // ============================================================
    
    /// <summary>
    /// Gets asset detail view by ID (includes all JOIN data)
    /// </summary>
    Task<AssetDetailView?> GetDetailByIdAsync(int assetId);
    
    /// <summary>
    /// Gets all assets as list view (for grid/list display)
    /// </summary>
    Task<IEnumerable<AssetListView>> GetAllListViewAsync();
    
    /// <summary>
    /// Gets assets by category ID as list view
    /// </summary>
    Task<IEnumerable<AssetListView>> GetByCategoryListViewAsync(int categoryId);
    
    /// <summary>
    /// Gets assets by office ID as list view
    /// </summary>
    Task<IEnumerable<AssetListView>> GetByOfficeListViewAsync(int officeId);
    
    /// <summary>
    /// Gets assets by current holder employee ID as list view
    /// </summary>
    Task<IEnumerable<AssetListView>> GetByHolderListViewAsync(int employeeId);
    
    /// <summary>
    /// Gets assets with expired warranty as list view
    /// </summary>
    Task<IEnumerable<AssetListView>> GetExpiredWarrantyListViewAsync();
    
    /// <summary>
    /// Gets assets with upcoming maintenance as list view
    /// </summary>
    Task<IEnumerable<AssetListView>> GetUpcomingMaintenanceListViewAsync(int daysAhead = 30);
    
    /// <summary>
    /// Gets assets filtered by status as list view
    /// </summary>
    Task<IEnumerable<AssetListView>> GetByStatusListViewAsync(string status);
    
    /// <summary>
    /// Searches assets by keyword
    /// </summary>
    Task<IEnumerable<AssetListView>> SearchAssetsAsync(string keyword, int limit = 10);
    
    // ============================================================
    // COUNT METHODS
    // ============================================================
    
    Task<int> GetTotalAssetsCountAsync();
    Task<int> GetAvailableAssetsCountAsync();
    Task<int> GetAssignedAssetsCountAsync();
    Task<int> GetAssetsOnLoanCountAsync();
    Task<int> GetAssetsInMaintenanceCountAsync();
    Task<int> GetDamagedAssetsCountAsync();
    Task<int> GetExpiredWarrantyCountAsync();
    Task<int> GetUpcomingMaintenanceCountAsync(int daysAhead = 30);
    Task<decimal> GetTotalAssetValueAsync();

    // ============================================================
    // NEW: AVAILABLE ASSETS FOR TRANSACTION
    // ============================================================
    
    /// <summary>
    /// Gets assets that are available for new transaction
    /// (Not assigned, not on loan, not in maintenance)
    /// </summary>
    Task<IEnumerable<AssetDropdownView>> GetAvailableAssetsForTransactionAsync();
    
    /// <summary>
    /// Checks if asset is available for new transaction
    /// </summary>
    Task<bool> IsAssetAvailableForTransactionAsync(int assetId);

    // ============================================================
    // NEW: ASSET STATUS BY DAMAGED/INACTIVE
    // ============================================================
    
    /// <summary>
    /// Gets damaged assets (asset condition = Damaged)
    /// </summary>
    Task<IEnumerable<AssetListView>> GetDamagedAssetsListViewAsync();
    
    /// <summary>
    /// Gets inactive assets (IsActive = false)
    /// </summary>
    Task<IEnumerable<AssetListView>> GetInactiveAssetsListViewAsync();

    // ============================================================
    // PAGINATION METHODS
    // ============================================================
    
    Task<PaginatedResult<AssetListView>> GetPagedListAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null);
    
    Task<IEnumerable<AssetDropdownView>> GetDropdownListAsync();
}