using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Office;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Office;

public interface IOfficeService
{
    Task<ServiceResult<OfficeDetailViewModel>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<OfficeListViewModel>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<OfficeListViewModel>>> GetActiveOnlyAsync();
    Task<ServiceResult<IEnumerable<OfficeListViewModel>>> GetSubOfficesAsync(int parentId);
    Task<ServiceResult<OfficeDetailViewModel>> CreateAsync(OfficeCreateViewModel model);
    Task<ServiceResult<OfficeDetailViewModel>> UpdateAsync(int id, OfficeUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);
    Task<ServiceResult<PaginatedResult<OfficeListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null);
}