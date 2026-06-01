using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Office;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Office;

/// <summary>
/// Service interface for Office business logic
/// </summary>
public interface IOfficeService
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    /// <summary>
    /// Gets an office by ID with all related data
    /// </summary>
    /// <param name="id">Office ID</param>
    /// <returns>Office detail view or not found result</returns>
    Task<ServiceResult<OfficeDetailView>> GetByIdAsync(int id);
    
    /// <summary>
    /// Gets all offices as list view
    /// </summary>
    /// <returns>Collection of office list views</returns>
    Task<ServiceResult<IEnumerable<OfficeListView>>> GetAllAsync();
    
    /// <summary>
    /// Gets active only offices as list view
    /// </summary>
    /// <returns>Collection of active office list views</returns>
    Task<ServiceResult<IEnumerable<OfficeListView>>> GetActiveOnlyAsync();
    
    /// <summary>
    /// Gets sub-offices by parent ID
    /// </summary>
    /// <param name="parentId">Parent office ID</param>
    /// <returns>Collection of sub-office list views</returns>
    Task<ServiceResult<IEnumerable<OfficeListView>>> GetSubOfficesAsync(int parentId);
    
    /// <summary>
    /// Creates a new office
    /// </summary>
    /// <param name="model">Office creation data</param>
    /// <returns>Created office detail view</returns>
    Task<ServiceResult<OfficeDetailView>> CreateAsync(OfficeCreateViewModel model);
    
    /// <summary>
    /// Updates an existing office
    /// </summary>
    /// <param name="id">Office ID</param>
    /// <param name="model">Office update data</param>
    /// <returns>Updated office detail view</returns>
    Task<ServiceResult<OfficeDetailView>> UpdateAsync(int id, OfficeUpdateViewModel model);
    
    /// <summary>
    /// Permanently deletes an office
    /// </summary>
    /// <param name="id">Office ID</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> DeleteAsync(int id);
    
    /// <summary>
    /// Soft deletes an office (sets IsActive = false)
    /// </summary>
    /// <param name="id">Office ID</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID & DROPDOWN
    // ============================================================

    /// <summary>
    /// Gets paginated list of offices for grid display
    /// </summary>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search keyword</param>
    /// <returns>Paginated result with office list views</returns>
    Task<ServiceResult<PaginatedResult<OfficeListView>>> GetGridDataAsync(int page, int pageSize, string? search = null);
    
    /// <summary>
    /// Gets office dropdown list (minimal data for selects)
    /// </summary>
    /// <returns>Collection of office dropdown views</returns>
    Task<ServiceResult<IEnumerable<OfficeDropdownView>>> GetDropdownListAsync();
}