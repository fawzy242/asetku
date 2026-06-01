using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Employee;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Employee;

/// <summary>
/// Service interface for Employee business logic
/// </summary>
public interface IEmployeeService
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    /// <summary>
    /// Gets an employee by ID with all related data
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <returns>Employee detail view or not found result</returns>
    Task<ServiceResult<EmployeeDetailView>> GetByIdAsync(int id);
    
    /// <summary>
    /// Gets all employees as list view
    /// </summary>
    /// <returns>Collection of employee list views</returns>
    Task<ServiceResult<IEnumerable<EmployeeListView>>> GetAllAsync();
    
    /// <summary>
    /// Gets employees by department ID
    /// </summary>
    /// <param name="departmentId">Department ID</param>
    /// <returns>Collection of employee list views</returns>
    Task<ServiceResult<IEnumerable<EmployeeListView>>> GetByDepartmentAsync(int departmentId);
    
    /// <summary>
    /// Gets employees by employment status
    /// </summary>
    /// <param name="employmentStatus">Employment status code</param>
    /// <returns>Collection of employee list views</returns>
    Task<ServiceResult<IEnumerable<EmployeeListView>>> GetByStatusAsync(int employmentStatus);
    
    /// <summary>
    /// Creates a new employee
    /// </summary>
    /// <param name="model">Employee creation data</param>
    /// <returns>Created employee detail view</returns>
    Task<ServiceResult<EmployeeDetailView>> CreateAsync(EmployeeCreateViewModel model);
    
    /// <summary>
    /// Updates an existing employee
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="model">Employee update data</param>
    /// <returns>Updated employee detail view</returns>
    Task<ServiceResult<EmployeeDetailView>> UpdateAsync(int id, EmployeeUpdateViewModel model);
    
    /// <summary>
    /// Permanently deletes an employee
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> DeleteAsync(int id);
    
    /// <summary>
    /// Soft deletes an employee (sets IsActive = false)
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID & SEARCH
    // ============================================================

    /// <summary>
    /// Gets paginated list of employees for grid display
    /// </summary>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search keyword</param>
    /// <param name="sortBy">Sort column name</param>
    /// <param name="sortDescending">True for descending sort</param>
    /// <param name="filters">Additional filters (isActive, departmentId)</param>
    /// <returns>Paginated result with employee list views</returns>
    Task<ServiceResult<PaginatedResult<EmployeeListView>>> GetGridDataAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null);

    // ============================================================
    // ASSET SUMMARY
    // ============================================================

    /// <summary>
    /// Gets complete asset summary for an employee (current assets + history)
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Employee asset summary view model</returns>
    Task<ServiceResult<EmployeeAssetSummaryViewModel>> GetAssetSummaryAsync(int employeeId);

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    /// <summary>
    /// Bulk activate or deactivate employees
    /// </summary>
    /// <param name="request">Bulk activation request with IDs and action</param>
    /// <returns>Number of employees affected</returns>
    Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request);
    
    // ============================================================
    // DROPDOWN
    // ============================================================

    /// <summary>
    /// Gets employee dropdown list (minimal data for selects)
    /// </summary>
    /// <returns>Collection of employee dropdown views</returns>
    Task<ServiceResult<IEnumerable<EmployeeDropdownView>>> GetDropdownListAsync();
}