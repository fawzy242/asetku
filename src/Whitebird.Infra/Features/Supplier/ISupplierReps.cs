using Whitebird.Domain.Features.Supplier;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Supplier;

/// <summary>
/// Repository interface for Supplier operations
/// </summary>
public interface ISupplierReps
{
    // ============================================================
    // RAW ENTITY METHODS (For internal Service use only - NOT for API)
    // ============================================================
    
    /// <summary>
    /// Gets supplier entity by ID (RAW - returns Entity, for internal use only)
    /// </summary>
    /// <param name="supplierId">Supplier ID</param>
    /// <returns>Supplier entity or null if not found</returns>
    Task<SupplierEntity?> GetByIdRawAsync(int supplierId);
    
    /// <summary>
    /// Checks if supplier name already exists
    /// </summary>
    /// <param name="supplierName">Supplier name to check</param>
    /// <param name="excludeSupplierId">Optional supplier ID to exclude (for updates)</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> IsSupplierNameExistsAsync(string supplierName, int? excludeSupplierId = null);
    
    /// <summary>
    /// Gets asset count for a supplier    /// </summary>
    /// <param name="supplierId">Supplier ID</param>
    /// <returns>Number of assets from this supplier</returns>
    Task<int> GetAssetCountAsync(int supplierId);
    
    // ============================================================
    // DETAIL VIEW METHODS (For API responses)
    // ============================================================
    
    /// <summary>
    /// Gets supplier detail view by ID (includes asset count)
    /// </summary>
    /// <param name="supplierId">Supplier ID</param>
    /// <returns>Supplier detail view or null if not found</returns>
    Task<SupplierDetailView?> GetDetailByIdAsync(int supplierId);
    
    /// <summary>
    /// Gets all suppliers as list view
    /// </summary>
    /// <returns>Collection of supplier list views</returns>
    Task<IEnumerable<SupplierListView>> GetAllListViewAsync();
    
    /// <summary>
    /// Gets active only suppliers as list view
    /// </summary>
    /// <returns>Collection of active supplier list views</returns>
    Task<IEnumerable<SupplierListView>> GetActiveOnlyListViewAsync();

    // ============================================================
    // PAGINATION METHODS
    // ============================================================
    
    /// <summary>
    /// Gets paged list of suppliers with filtering and search
    /// </summary>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search keyword</param>
    /// <param name="sortBy">Sort column name</param>
    /// <param name="sortDescending">True for descending sort</param>
    /// <param name="filters">Additional filters (isActive, etc)</param>
    /// <returns>Paginated result with supplier list views</returns>
    Task<PaginatedResult<SupplierListView>> GetPagedListAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null);
    
    /// <summary>
    /// Gets supplier dropdown list (minimal data for selects)
    /// </summary>
    /// <returns>Collection of supplier dropdown views</returns>
    Task<IEnumerable<SupplierDropdownView>> GetDropdownListAsync();
}