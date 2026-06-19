using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.AssetTransaction;

/// <summary>
/// Service interface for Asset Transaction business logic
/// </summary>
public interface IAssetTransactionService
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    Task<ServiceResult<AssetTransactionDetailView>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetByAssetIdAsync(int assetId);
    Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetByEmployeeIdAsync(int employeeId);
    Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetByApprovalStatusAsync(bool? approved);
    Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetPendingApprovalsAsync();

    // ============================================================
    // CREATE/UPDATE/DELETE
    // ============================================================

    Task<ServiceResult<AssetTransactionDetailView>> CreateAsync(AssetTransactionCreateViewModel model);
    Task<ServiceResult<AssetTransactionDetailView>> UpdateAsync(int id, AssetTransactionUpdateViewModel model);
    Task<ServiceResult> ApproveAsync(int id, AssetTransactionApproveViewModel model);
    Task<ServiceResult> ReturnAssetAsync(AssetReturnViewModel model);
    Task<ServiceResult> CancelAsync(int id);

    // ============================================================
    // NEW: SHORTCUT METHODS
    // ============================================================

    /// <summary>
    /// Creates a RETURN transaction as a shortcut (for HANDOVER, LOAN, MAINTENANCE)
    /// </summary>
    Task<ServiceResult<AssetTransactionDetailView>> CreateReturnTransactionAsync(int sourceTransactionId, AssetReturnViewModel model);
    
    /// <summary>
    /// Creates a POST_MAINTENANCE transaction as a shortcut (for MAINTENANCE)
    /// </summary>
    Task<ServiceResult<AssetTransactionDetailView>> CreatePostMaintenanceTransactionAsync(int sourceTransactionId, PostMaintenanceViewModel model);

    // ============================================================
    // GRID & TRACKING
    // ============================================================

    Task<ServiceResult<PaginatedResult<AssetTransactionListView>>> GetGridDataAsync(int page, int pageSize, string? search = null, bool? approved = null, int? assetId = null);

    // ============================================================
    // ACTIVE LOANS
    // ============================================================

    Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetActiveLoansAsync();
    Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetOverdueLoansAsync();
    
    // ============================================================
    // PAIRED TRANSACTIONS
    // ============================================================

    Task<ServiceResult<IEnumerable<AssetTransactionDropdownView>>> GetAvailablePairedTransactionsAsync(int assetId, int transactionType);
}