using Whitebird.Domain.Features.Asset.View;
using Whitebird.App.Features.Common.Service;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Asset.Interfaces;

public interface IAssetService
{
    Task<ServiceResult<AssetDetailViewModel>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetByCategoryAsync(int categoryId);
    Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetByStatusAsync(string status);
    Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetByHolderAsync(int employeeId);
    Task<ServiceResult<AssetDetailViewModel>> CreateAsync(AssetCreateViewModel model);
    Task<ServiceResult<AssetDetailViewModel>> UpdateAsync(int id, AssetUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);
    Task<ServiceResult<PaginatedResult<AssetListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null, string? sortBy = null, bool sortDescending = false, Dictionary<string, object>? filters = null);
    Task<ServiceResult<IEnumerable<AssetListViewModel>>> SearchAsync(string keyword);
    Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetExpiredWarrantyAsync();
    Task<ServiceResult<IEnumerable<AssetListViewModel>>> GetUpcomingMaintenanceAsync(int daysAhead = 30);
    Task<ServiceResult<DashboardStatsViewModel>> GetDashboardStatsAsync();
}