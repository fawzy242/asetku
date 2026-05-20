using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Department;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Department;

public interface IDepartmentService
{
    Task<ServiceResult<DepartmentDetailViewModel>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<DepartmentListViewModel>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<DepartmentListViewModel>>> GetActiveOnlyAsync();
    Task<ServiceResult<DepartmentDetailViewModel>> CreateAsync(DepartmentCreateViewModel model);
    Task<ServiceResult<DepartmentDetailViewModel>> UpdateAsync(int id, DepartmentUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);
    Task<ServiceResult<PaginatedResult<DepartmentListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null);
}