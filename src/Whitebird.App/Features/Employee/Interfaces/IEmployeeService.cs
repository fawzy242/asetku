using Whitebird.Domain.Features.Employee.View;
using Whitebird.App.Features.Common.Service;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Employee.Interfaces;

public interface IEmployeeService
{
    Task<ServiceResult<EmployeeDetailViewModel>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<EmployeeListViewModel>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<EmployeeListViewModel>>> GetByDepartmentAsync(string department);
    Task<ServiceResult<IEnumerable<EmployeeListViewModel>>> GetByStatusAsync(string status);
    Task<ServiceResult<EmployeeDetailViewModel>> CreateAsync(EmployeeCreateViewModel model);
    Task<ServiceResult<EmployeeDetailViewModel>> UpdateAsync(int id, EmployeeUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);
    Task<ServiceResult<PaginatedResult<EmployeeListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false);
}