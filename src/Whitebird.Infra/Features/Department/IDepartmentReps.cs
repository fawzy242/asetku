using Whitebird.Domain.Features.Department;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Department;

/// <summary>
/// Repository interface for Department operations
/// </summary>
public interface IDepartmentReps
{
    // ============================================================
    // RAW ENTITY METHODS
    // ============================================================
    
    Task<DepartmentEntity?> GetByIdRawAsync(int departmentId);
    Task<bool> IsDepartmentNameExistsAsync(string departmentName, int? excludeDepartmentId = null);
    Task<bool> IsDepartmentCodeExistsAsync(string departmentCode, int? excludeDepartmentId = null);
    Task<int> GetEmployeeCountAsync(int departmentId);
    
    // ============================================================
    // DETAIL VIEW METHODS
    // ============================================================
    
    Task<DepartmentDetailView?> GetDetailByIdAsync(int departmentId);
    Task<IEnumerable<DepartmentListView>> GetAllListViewAsync();

    // ============================================================
    // PAGINATION
    // ============================================================
    
    Task<PaginatedResult<DepartmentListView>> GetPagedListAsync(DepartmentGridRequest request);
    
    // ============================================================
    // DROPDOWN
    // ============================================================
    
    Task<IEnumerable<DepartmentDropdownView>> GetDropdownListAsync();
}