using Whitebird.Domain.Features.AssetTransaction.View;
using Whitebird.App.Features.Common.Service;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.AssetTransaction.Interfaces;

public interface IAssetTransactionService
{
    Task<ServiceResult<AssetTransactionDetailViewModel>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetByAssetIdAsync(int assetId);
    Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetByEmployeeIdAsync(int employeeId);
    Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetByStatusAsync(string status);
    Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetPendingApprovalsAsync();
    Task<ServiceResult<AssetTransactionDetailViewModel>> CreateAsync(AssetTransactionCreateViewModel model);
    Task<ServiceResult<AssetTransactionDetailViewModel>> UpdateAsync(int id, AssetTransactionUpdateViewModel model);
    Task<ServiceResult> ApproveAsync(int id, AssetTransactionApproveViewModel model);
    Task<ServiceResult> ReturnAssetAsync(AssetReturnViewModel model);
    Task<ServiceResult> CancelAsync(int id);
    Task<ServiceResult<PaginatedResult<AssetTransactionListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null, string? status = null, int? assetId = null);
}