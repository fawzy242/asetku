using Whitebird.Domain.Features.Supplier;
using Whitebird.App.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Supplier;

/// <summary>
/// Service interface for Supplier business logic
/// </summary>
public interface ISupplierService
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    /// <summary>
    /// Gets a supplier by ID with all related data
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <returns>Supplier detail view or not found result</returns>
    Task<ServiceResult<SupplierDetailView>> GetByIdAsync(int id);
    
    /// <summary>
    /// Gets all suppliers as list view
    /// </summary>
    /// <returns>Collection of supplier list views</returns>
    Task<ServiceResult<IEnumerable<SupplierListView>>> GetAllAsync();
    
    /// <summary>
    /// Gets active only suppliers as list view
    /// </summary>
    /// <returns>Collection of active supplier list views</returns>
    Task<ServiceResult<IEnumerable<SupplierListView>>> GetActiveOnlyAsync();
    
    /// <summary>
    /// Creates a new supplier
    /// </summary>
    /// <param name="model">Supplier creation data</param>
    /// <returns>Created supplier detail view</returns>
    Task<ServiceResult<SupplierDetailView>> CreateAsync(SupplierCreateViewModel model);
    
    /// <summary>
    /// Updates an existing supplier
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <param name="model">Supplier update data</param>
    /// <returns>Updated supplier detail view</returns>
    Task<ServiceResult<SupplierDetailView>> UpdateAsync(int id, SupplierUpdateViewModel model);
    
    /// <summary>
    /// Permanently deletes a supplier
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> DeleteAsync(int id);
    
    /// <summary>
    /// Soft deletes a supplier (sets IsActive = false)
    /// </summary>
    /// <param name="id">Supplier ID</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID & DROPDOWN
    // ============================================================

    /// <summary>
    /// Gets paginated list of suppliers for grid display
    /// </summary>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search keyword</param>
    /// <returns>Paginated result with supplier list views</returns>
    Task<ServiceResult<PaginatedResult<SupplierListView>>> GetGridDataAsync(int page, int pageSize, string? search = null);
    
    /// <summary>
    /// Gets supplier dropdown list (minimal data for selects)
    /// </summary>
    /// <returns>Collection of supplier dropdown views</returns>
    Task<ServiceResult<IEnumerable<SupplierDropdownView>>> GetDropdownListAsync();
}