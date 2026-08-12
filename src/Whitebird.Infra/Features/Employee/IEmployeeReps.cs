using Whitebird.Domain.Features.Employee;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Employee;

/// <summary>
/// Repository interface for Employee operations
/// </summary>
public interface IEmployeeReps
{
    // ============================================================
    // RAW ENTITY METHODS
    // ============================================================
    
    Task<EmployeeEntity?> GetByIdRawAsync(int employeeId);
    Task<bool> IsEmployeeCodeExistsAsync(string employeeCode, int? excludeEmployeeId = null);
    
    // ============================================================
    // DETAIL VIEW METHODS
    // ============================================================
    
    Task<EmployeeDetailView?> GetDetailByIdAsync(int employeeId);
    Task<IEnumerable<EmployeeListView>> GetAllListViewAsync();
    
    // ============================================================
    // PAGINATION
    // ============================================================
    
    Task<PaginatedResult<EmployeeListView>> GetPagedListAsync(EmployeeGridRequest request);
    
    // ============================================================
    // DROPDOWN
    // ============================================================
    
    Task<IEnumerable<EmployeeDropdownView>> GetDropdownListAsync();
}