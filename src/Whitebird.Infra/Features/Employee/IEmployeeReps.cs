using Whitebird.Domain.Features.Employee;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Employee;

/// <summary>
/// Repository interface for Employee operations
/// </summary>
public interface IEmployeeReps
{
    // ============================================================
    // RAW ENTITY METHODS (For internal Service use only - NOT for API)
    // ============================================================
    
    /// <summary>
    /// Gets employee entity by ID (RAW - returns Entity, for internal use only)
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Employee entity or null if not found</returns>
    Task<EmployeeEntity?> GetByIdRawAsync(int employeeId);
    
    /// <summary>
    /// Checks if employee code already exists
    /// </summary>
    /// <param name="employeeCode">Employee code to check</param>
    /// <param name="excludeEmployeeId">Optional employee ID to exclude (for updates)</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> IsEmployeeCodeExistsAsync(string employeeCode, int? excludeEmployeeId = null);
    
    // ============================================================
    // DETAIL VIEW METHODS (For API responses)
    // ============================================================
    
    /// <summary>
    /// Gets employee detail view by ID (includes all JOIN data)
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Employee detail view or null if not found</returns>
    Task<EmployeeDetailView?> GetDetailByIdAsync(int employeeId);
    
    /// <summary>
    /// Gets all employees as list view
    /// </summary>
    /// <returns>Collection of employee list views</returns>
    Task<IEnumerable<EmployeeListView>> GetAllListViewAsync();
    
    /// <summary>
    /// Gets active only employees as list view
    /// </summary>
    /// <returns>Collection of active employee list views</returns>
    Task<IEnumerable<EmployeeListView>> GetActiveOnlyListViewAsync();
    
    /// <summary>
    /// Gets employees by department ID as list view
    /// </summary>
    /// <param name="departmentId">Department ID</param>
    /// <returns>Collection of employee list views</returns>
    Task<IEnumerable<EmployeeListView>> GetByDepartmentIdListViewAsync(int departmentId);
    
    /// <summary>
    /// Gets employees by employment status as list view
    /// </summary>
    /// <param name="employmentStatus">Employment status code</param>
    /// <returns>Collection of employee list views</returns>
    Task<IEnumerable<EmployeeListView>> GetByEmploymentStatusListViewAsync(int employmentStatus);
    
    // ============================================================
    // ASSET STATISTICS (For employee asset summary)
    // ============================================================
    
    /// <summary>
    /// Gets count of active assets held by employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Count of active assets</returns>
    Task<int> GetActiveAssetsCountAsync(int employeeId);
    
    /// <summary>
    /// Gets count of assets on loan to employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Count of assets on loan</returns>
    Task<int> GetAssetsOnLoanCountAsync(int employeeId);
    
    /// <summary>
    /// Gets count of overdue loans for employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Count of overdue loans</returns>
    Task<int> GetOverdueLoansCountAsync(int employeeId);
    
    /// <summary>
    /// Gets total historical assets associated with employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Total historical assets count</returns>
    Task<int> GetTotalHistoricalAssetsAsync(int employeeId);
    
    /// <summary>
    /// Gets count of assets returned by employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Count of returned assets</returns>
    Task<int> GetReturnedAssetsCountAsync(int employeeId);
    
    /// <summary>
    /// Gets count of damaged returns by employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Count of damaged returns</returns>
    Task<int> GetDamagedReturnsCountAsync(int employeeId);

    // ============================================================
    // ASSET SUMMARY (Full employee asset summary)
    // ============================================================
    
    /// <summary>
    /// Gets complete asset summary for an employee (current assets + history)
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Employee asset summary view or null if employee not found</returns>
    Task<EmployeeAssetSummaryView?> GetAssetSummaryByEmployeeIdAsync(int employeeId);

    // ============================================================
    // PAGINATION METHODS
    // ============================================================
    
    /// <summary>
    /// Gets paged list of employees with filtering, sorting, and search
    /// </summary>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search keyword</param>
    /// <param name="sortBy">Sort column name</param>
    /// <param name="sortDescending">True for descending sort</param>
    /// <param name="filters">Additional filters (isActive, departmentId, etc)</param>
    /// <returns>Paginated result with employee list views</returns>
    Task<PaginatedResult<EmployeeListView>> GetPagedListAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null);
    
    /// <summary>
    /// Gets employee dropdown list (minimal data for selects)
    /// </summary>
    /// <returns>Collection of employee dropdown views</returns>
    Task<IEnumerable<EmployeeDropdownView>> GetDropdownListAsync();
}