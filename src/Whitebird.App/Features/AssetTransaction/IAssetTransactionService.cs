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

    /// <summary>
    /// Gets a transaction by ID with all related data
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <returns>Transaction detail view or not found result</returns>
    Task<ServiceResult<AssetTransactionDetailView>> GetByIdAsync(int id);
    
    /// <summary>
    /// Gets all transactions as list view
    /// </summary>
    /// <returns>Collection of transaction list views</returns>
    Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetAllAsync();
    
    /// <summary>
    /// Gets transactions by asset ID
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    /// <returns>Collection of transaction list views</returns>
    Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetByAssetIdAsync(int assetId);
    
    /// <summary>
    /// Gets transactions by employee ID (as sender or receiver)
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Collection of transaction list views</returns>
    Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetByEmployeeIdAsync(int employeeId);
    
    /// <summary>
    /// Gets transactions by approval status
    /// </summary>
    /// <param name="approved">True for approved, false for rejected, null for pending</param>
    /// <returns>Collection of transaction list views</returns>
    Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetByApprovalStatusAsync(bool? approved);
    
    /// <summary>
    /// Gets pending approval transactions
    /// </summary>
    /// <returns>Collection of pending transaction list views</returns>
    Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetPendingApprovalsAsync();

    // ============================================================
    // CREATE/UPDATE/DELETE
    // ============================================================

    /// <summary>
    /// Creates a new transaction (pending approval)
    /// </summary>
    /// <param name="model">Transaction creation data</param>
    /// <returns>Created transaction detail view</returns>
    Task<ServiceResult<AssetTransactionDetailView>> CreateAsync(AssetTransactionCreateViewModel model);
    
    /// <summary>
    /// Updates an existing transaction (only if not approved/rejected)
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <param name="model">Transaction update data</param>
    /// <returns>Updated transaction detail view</returns>
    Task<ServiceResult<AssetTransactionDetailView>> UpdateAsync(int id, AssetTransactionUpdateViewModel model);
    
    /// <summary>
    /// Approves or rejects a pending transaction
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <param name="model">Approval decision data</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> ApproveAsync(int id, AssetTransactionApproveViewModel model);
    
    /// <summary>
    /// Returns an asset from a loan or assigned transaction
    /// </summary>
    /// <param name="model">Return asset data</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> ReturnAssetAsync(AssetReturnViewModel model);
    
    /// <summary>
    /// Cancels a pending transaction
    /// </summary>
    /// <param name="id">Transaction ID</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> CancelAsync(int id);

    // ============================================================
    // GRID & TRACKING
    // ============================================================

    /// <summary>
    /// Gets paginated list of transactions for grid display
    /// </summary>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search keyword</param>
    /// <param name="approved">Approval status filter</param>
    /// <param name="assetId">Asset ID filter</param>
    /// <returns>Paginated result with transaction list views</returns>
    Task<ServiceResult<PaginatedResult<AssetTransactionListView>>> GetGridDataAsync(
        int page, int pageSize, string? search = null, bool? approved = null, int? assetId = null);

    // ============================================================
    // ACTIVE LOANS
    // ============================================================

    /// <summary>
    /// Gets all active loans (not yet returned)
    /// </summary>
    /// <returns>Collection of active loan list views</returns>
    Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetActiveLoansAsync();
    
    /// <summary>
    /// Gets all overdue loans
    /// </summary>
    /// <returns>Collection of overdue loan list views</returns>
    Task<ServiceResult<IEnumerable<AssetTransactionListView>>> GetOverdueLoansAsync();
    
    // ============================================================
    // PAIRED TRANSACTIONS
    // ============================================================

    /// <summary>
    /// Gets available transactions for pairing (LOAN or MAINTENANCE that are not yet paired)
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    /// <param name="transactionType">Transaction type to pair with (LOAN or MAINTENANCE)</param>
    /// <returns>Collection of transaction dropdown views</returns>
    Task<ServiceResult<IEnumerable<AssetTransactionDropdownView>>> GetAvailablePairedTransactionsAsync(int assetId, int transactionType);
}