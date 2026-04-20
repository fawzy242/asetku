using Whitebird.Domain.Features.Location.View;
using Whitebird.App.Features.Common.Service;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Location.Interfaces;

public interface ILocationService
{
    Task<ServiceResult<LocationDetailViewModel>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<LocationListViewModel>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<LocationListViewModel>>> GetActiveOnlyAsync();
    Task<ServiceResult<IEnumerable<LocationListViewModel>>> GetSubLocationsAsync(int parentId);
    Task<ServiceResult<LocationDetailViewModel>> CreateAsync(LocationCreateViewModel model);
    Task<ServiceResult<LocationDetailViewModel>> UpdateAsync(int id, LocationUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);
    Task<ServiceResult<PaginatedResult<LocationListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null);
}