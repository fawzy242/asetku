using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.AssetTransaction;

public interface IAssetTransactionService
{
    // Basic CRUD
    Task<ServiceResult<AssetTransactionDetailViewModel>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetByAssetIdAsync(int assetId);
    Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetByEmployeeIdAsync(int employeeId);
    Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetByApprovalStatusAsync(bool? approved);
    Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetPendingApprovalsAsync();

    // Create/Update/Delete
    Task<ServiceResult<AssetTransactionDetailViewModel>> CreateAsync(AssetTransactionCreateViewModel model);
    Task<ServiceResult<AssetTransactionDetailViewModel>> UpdateAsync(int id, AssetTransactionUpdateViewModel model);
    Task<ServiceResult> ApproveAsync(int id, AssetTransactionApproveViewModel model);
    Task<ServiceResult> ReturnAssetAsync(AssetReturnViewModel model);
    Task<ServiceResult> CancelAsync(int id);

    // Grid & Tracking
    Task<ServiceResult<PaginatedResult<AssetTransactionListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null, bool? approved = null, int? assetId = null);

    // Active loans
    Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetActiveLoansAsync();
    Task<ServiceResult<IEnumerable<AssetTransactionListViewModel>>> GetOverdueLoansAsync();
}