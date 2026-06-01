using Whitebird.Domain.Features.Department;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Department;

/// <summary>
/// Repository interface for Department operations
/// </summary>
public interface IDepartmentReps
{
    // ============================================================
    // RAW ENTITY METHODS (For internal Service use only - NOT for API)
    // ============================================================
    
    /// <summary>
    /// Gets department entity by ID (RAW - returns Entity, for internal use only)
    /// </summary>
    /// <param name="departmentId">Department ID</param>
    /// <returns>Department entity or null if not found</returns>
    Task<DepartmentEntity?> GetByIdRawAsync(int departmentId);
    
    /// <summary>
    /// Checks if department name already exists
    /// </summary>
    /// <param name="departmentName">Department name to check</param>
    /// <param name="excludeDepartmentId">Optional department ID to exclude (for updates)</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> IsDepartmentNameExistsAsync(string departmentName, int? excludeDepartmentId = null);
    
    /// <summary>
    /// Checks if department code already exists
    /// </summary>
    /// <param name="departmentCode">Department code to check</param>
    /// <param name="excludeDepartmentId">Optional department ID to exclude (for updates)</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> IsDepartmentCodeExistsAsync(string departmentCode, int? excludeDepartmentId = null);
    
    /// <summary>
    /// Gets employee count for a department
    /// </summary>
    /// <param name="departmentId">Department ID</param>
    /// <returns>Number of employees in department</returns>
    Task<int> GetEmployeeCountAsync(int departmentId);
    
    // ============================================================
    // DETAIL VIEW METHODS (For API responses)
    // ============================================================
    
    /// <summary>
    /// Gets department detail view by ID (includes employee count)
    /// </summary>
    /// <param name="departmentId">Department ID</param>
    /// <returns>Department detail view or null if not found</returns>
    Task<DepartmentDetailView?> GetDetailByIdAsync(int departmentId);
    
    /// <summary>
    /// Gets all departments as list view
    /// </summary>
    /// <returns>Collection of department list views</returns>
    Task<IEnumerable<DepartmentListView>> GetAllListViewAsync();
    
    /// <summary>
    /// Gets active only departments as list view
    /// </summary>
    /// <returns>Collection of active department list views</returns>
    Task<IEnumerable<DepartmentListView>> GetActiveOnlyListViewAsync();

    // ============================================================
    // PAGINATION METHODS
    // ============================================================
    
    /// <summary>
    /// Gets paged list of departments with filtering and search
    /// </summary>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search keyword</param>
    /// <param name="sortBy">Sort column name</param>
    /// <param name="sortDescending">True for descending sort</param>
    /// <param name="filters">Additional filters (isActive, etc)</param>
    /// <returns>Paginated result with department list views</returns>
    Task<PaginatedResult<DepartmentListView>> GetPagedListAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null);
    
    /// <summary>
    /// Gets department dropdown list (minimal data for selects)
    /// </summary>
    /// <returns>Collection of department dropdown views</returns>
    Task<IEnumerable<DepartmentDropdownView>> GetDropdownListAsync();
}