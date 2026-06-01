using Whitebird.Domain.Features.Office;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Office;

/// <summary>
/// Repository interface for Office operations
/// </summary>
public interface IOfficeReps
{
    // ============================================================
    // RAW ENTITY METHODS (For internal Service use only - NOT for API)
    // ============================================================
    
    /// <summary>
    /// Gets office entity by ID (RAW - returns Entity, for internal use only)
    /// </summary>
    /// <param name="officeId">Office ID</param>
    /// <returns>Office entity or null if not found</returns>
    Task<OfficeEntity?> GetByIdRawAsync(int officeId);
    
    /// <summary>
    /// Checks if office code already exists
    /// </summary>
    /// <param name="officeCode">Office code to check</param>
    /// <param name="excludeOfficeId">Optional office ID to exclude (for updates)</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> IsOfficeCodeExistsAsync(string officeCode, int? excludeOfficeId = null);
    
    /// <summary>
    /// Checks if office name already exists
    /// </summary>
    /// <param name="officeName">Office name to check</param>
    /// <param name="excludeOfficeId">Optional office ID to exclude (for updates)</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> IsOfficeNameExistsAsync(string officeName, int? excludeOfficeId = null);
    
    /// <summary>
    /// Gets child count for an office
    /// </summary>
    /// <param name="officeId">Parent office ID</param>
    /// <returns>Number of child offices</returns>
    Task<int> GetChildCountAsync(int officeId);
    
    // ============================================================
    // DETAIL VIEW METHODS (For API responses)
    // ============================================================
    
    /// <summary>
    /// Gets office detail view by ID (includes parent info, child count, office type name)
    /// </summary>
    /// <param name="officeId">Office ID</param>
    /// <returns>Office detail view or null if not found</returns>
    Task<OfficeDetailView?> GetDetailByIdAsync(int officeId);
    
    /// <summary>
    /// Gets all offices as list view
    /// </summary>
    /// <returns>Collection of office list views</returns>
    Task<IEnumerable<OfficeListView>> GetAllListViewAsync();
    
    /// <summary>
    /// Gets active only offices as list view
    /// </summary>
    /// <returns>Collection of active office list views</returns>
    Task<IEnumerable<OfficeListView>> GetActiveOnlyListViewAsync();
    
    /// <summary>
    /// Gets sub-offices by parent ID as list view
    /// </summary>
    /// <param name="parentOfficeId">Parent office ID</param>
    /// <returns>Collection of sub-office list views</returns>
    Task<IEnumerable<OfficeListView>> GetSubOfficesListViewAsync(int parentOfficeId);

    // ============================================================
    // PAGINATION METHODS
    // ============================================================
    
    /// <summary>
    /// Gets paged list of offices with filtering and search
    /// </summary>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search keyword</param>
    /// <param name="sortBy">Sort column name</param>
    /// <param name="sortDescending">True for descending sort</param>
    /// <param name="filters">Additional filters (isActive, officeType, etc)</param>
    /// <returns>Paginated result with office list views</returns>
    Task<PaginatedResult<OfficeListView>> GetPagedListAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null);
    
    /// <summary>
    /// Gets office dropdown list (minimal data for selects)
    /// </summary>
    /// <returns>Collection of office dropdown views</returns>
    Task<IEnumerable<OfficeDropdownView>> GetDropdownListAsync();
}