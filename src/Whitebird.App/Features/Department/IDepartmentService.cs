using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Department;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Department;

/// <summary>
/// Service interface for Department business logic
/// </summary>
public interface IDepartmentService
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    /// <summary>
    /// Gets a department by ID with all related data
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <returns>Department detail view or not found result</returns>
    Task<ServiceResult<DepartmentDetailView>> GetByIdAsync(int id);
    
    /// <summary>
    /// Gets all departments as list view
    /// </summary>
    /// <returns>Collection of department list views</returns>
    Task<ServiceResult<IEnumerable<DepartmentListView>>> GetAllAsync();
    
    /// <summary>
    /// Gets active only departments as list view
    /// </summary>
    /// <returns>Collection of active department list views</returns>
    Task<ServiceResult<IEnumerable<DepartmentListView>>> GetActiveOnlyAsync();
    
    /// <summary>
    /// Creates a new department
    /// </summary>
    /// <param name="model">Department creation data</param>
    /// <returns>Created department detail view</returns>
    Task<ServiceResult<DepartmentDetailView>> CreateAsync(DepartmentCreateViewModel model);
    
    /// <summary>
    /// Updates an existing department
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <param name="model">Department update data</param>
    /// <returns>Updated department detail view</returns>
    Task<ServiceResult<DepartmentDetailView>> UpdateAsync(int id, DepartmentUpdateViewModel model);
    
    /// <summary>
    /// Permanently deletes a department
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> DeleteAsync(int id);
    
    /// <summary>
    /// Soft deletes a department (sets IsActive = false)
    /// </summary>
    /// <param name="id">Department ID</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID & DROPDOWN
    // ============================================================

    /// <summary>
    /// Gets paginated list of departments for grid display
    /// </summary>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search keyword</param>
    /// <returns>Paginated result with department list views</returns>
    Task<ServiceResult<PaginatedResult<DepartmentListView>>> GetGridDataAsync(int page, int pageSize, string? search = null);
    
    /// <summary>
    /// Gets department dropdown list (minimal data for selects)
    /// </summary>
    /// <returns>Collection of department dropdown views</returns>
    Task<ServiceResult<IEnumerable<DepartmentDropdownView>>> GetDropdownListAsync();
}