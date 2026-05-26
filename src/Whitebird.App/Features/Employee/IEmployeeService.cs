using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Employee;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Employee;

public interface IEmployeeService
{
    // Basic CRUD
    Task<ServiceResult<EmployeeDetailViewModel>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<EmployeeListViewModel>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<EmployeeListViewModel>>> GetByDepartmentAsync(int departmentId);
    Task<ServiceResult<IEnumerable<EmployeeListViewModel>>> GetByStatusAsync(int employmentStatus);
    Task<ServiceResult<EmployeeDetailViewModel>> CreateAsync(EmployeeCreateViewModel model);
    Task<ServiceResult<EmployeeDetailViewModel>> UpdateAsync(int id, EmployeeUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // Grid & Search (UPDATED - support pagination)
    Task<ServiceResult<PaginatedResult<EmployeeListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false, Dictionary<string, object>? filters = null);

    // Asset Summary
    Task<ServiceResult<EmployeeAssetSummaryViewModel>> GetAssetSummaryAsync(int employeeId);

    // Bulk operations
    Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request);
}