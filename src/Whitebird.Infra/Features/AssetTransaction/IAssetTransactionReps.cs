using Whitebird.Domain.Features.AssetTransaction;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.AssetTransaction;

/// <summary>
/// Repository interface for Asset Transaction operations
/// </summary>
public interface IAssetTransactionReps
{
    // ============================================================
    // RAW ENTITY METHODS (For internal Service use only - NOT for API)
    // ============================================================
    
    /// <summary>
    /// Gets asset transaction entity by ID (RAW - returns Entity, for internal use only)
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <returns>Asset transaction entity or null if not found</returns>
    Task<AssetTransactionEntity?> GetByIdRawAsync(int transactionId);
    
    /// <summary>
    /// Gets transaction count for a specific asset
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    /// <returns>Transaction count</returns>
    Task<int> GetTransactionCountByAssetAsync(int assetId);
    
    /// <summary>
    /// Checks if there's an open paired transaction for an asset
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    /// <param name="transactionType">Transaction type to check (LOAN or MAINTENANCE)</param>
    /// <returns>True if open transaction exists</returns>
    Task<bool> HasOpenPairedTransactionAsync(int assetId, int transactionType);
    
    // ============================================================
    // DETAIL VIEW METHODS (For API responses)
    // ============================================================
    
    /// <summary>
    /// Gets asset transaction detail view by ID (includes all JOIN data)
    /// </summary>
    /// <param name="transactionId">Transaction ID</param>
    /// <returns>Asset transaction detail view or null if not found</returns>
    Task<AssetTransactionDetailView?> GetDetailByIdAsync(int transactionId);
    
    /// <summary>
    /// Gets all transactions as list view
    /// </summary>
    /// <returns>Collection of transaction list views</returns>
    Task<IEnumerable<AssetTransactionListView>> GetAllListViewAsync();
    
    /// <summary>
    /// Gets transactions by asset ID as list view
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    /// <returns>Collection of transaction list views</returns>
    Task<IEnumerable<AssetTransactionListView>> GetByAssetIdListViewAsync(int assetId);
    
    /// <summary>
    /// Gets transactions by employee ID (as sender or receiver) as list view
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Collection of transaction list views</returns>
    Task<IEnumerable<AssetTransactionListView>> GetByEmployeeIdListViewAsync(int employeeId);
    
    /// <summary>
    /// Gets transactions by approval status as list view
    /// </summary>
    /// <param name="approved">True for approved, false for rejected, null for pending</param>
    /// <returns>Collection of transaction list views</returns>
    Task<IEnumerable<AssetTransactionListView>> GetByApprovalStatusListViewAsync(bool? approved);
    
    /// <summary>
    /// Gets pending approval transactions as list view
    /// </summary>
    /// <returns>Collection of pending transaction list views</returns>
    Task<IEnumerable<AssetTransactionListView>> GetPendingApprovalsListViewAsync();
    
    /// <summary>
    /// Gets approved transactions as list view
    /// </summary>
    /// <returns>Collection of approved transaction list views</returns>
    Task<IEnumerable<AssetTransactionListView>> GetApprovedListViewAsync();
    
    /// <summary>
    /// Gets rejected transactions as list view
    /// </summary>
    /// <returns>Collection of rejected transaction list views</returns>
    Task<IEnumerable<AssetTransactionListView>> GetRejectedListViewAsync();
    
    /// <summary>
    /// Gets active loans as list view (ExpectedReturnDate >= GETDATE())
    /// </summary>
    /// <returns>Collection of active loan list views</returns>
    Task<IEnumerable<AssetTransactionListView>> GetActiveLoansListViewAsync();
    
    /// <summary>
    /// Gets overdue loans as list view (ExpectedReturnDate < GETDATE())
    /// </summary>
    /// <returns>Collection of overdue loan list views</returns>
    Task<IEnumerable<AssetTransactionListView>> GetOverdueLoansListViewAsync();
    
    /// <summary>
    /// Gets transactions within date range as list view
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <returns>Collection of transaction list views</returns>
    Task<IEnumerable<AssetTransactionListView>> GetByDateRangeListViewAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Gets active transaction for an asset (the most recent non-closed transaction)
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    /// <returns>Active transaction list view or null</returns>
    Task<AssetTransactionListView?> GetActiveTransactionByAssetIdAsync(int assetId);
    
    /// <summary>
    /// Gets paired transaction (LOAN_RETURN for LOAN, or POST_MAINTENANCE for MAINTENANCE)
    /// </summary>
    /// <param name="transactionId">Source transaction ID</param>
    /// <returns>Paired transaction entity or null</returns>
    Task<AssetTransactionEntity?> GetPairedTransactionAsync(int transactionId);
    
    /// <summary>
    /// Gets asset transaction history (all transactions for an asset)
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    /// <returns>Collection of transaction list views</returns>
    Task<IEnumerable<AssetTransactionListView>> GetAssetTransactionHistoryAsync(int assetId);
    
    /// <summary>
    /// Gets employee transaction history (all transactions where employee is sender or receiver)
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <returns>Collection of transaction list views</returns>
    Task<IEnumerable<AssetTransactionListView>> GetEmployeeTransactionHistoryAsync(int employeeId);

    // ============================================================
    // PAGINATION METHODS
    // ============================================================
    
    /// <summary>
    /// Gets paged list of transactions with filtering
    /// </summary>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search keyword</param>
    /// <param name="approved">Approval status filter</param>
    /// <param name="assetId">Asset ID filter</param>
    /// <returns>Paginated result with transaction list views</returns>
    Task<PaginatedResult<AssetTransactionListView>> GetPagedListAsync(
        int page, int pageSize, string? search = null, bool? approved = null, int? assetId = null, DateTime? startDate = null, DateTime? endDate = null);
    
    /// <summary>
    /// Gets paged list of pending approvals
    /// </summary>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search keyword</param>
    /// <param name="assetId">Asset ID filter</param>
    /// <returns>Paginated result with pending transaction list views</returns>
    Task<PaginatedResult<AssetTransactionListView>> GetPendingPagedListAsync(
        int page, int pageSize, string? search = null, int? assetId = null);
    
    /// <summary>
    /// Gets active loans list (alias for GetActiveLoansListViewAsync)
    /// </summary>
    /// <returns>Collection of active loan list views</returns>
    Task<IEnumerable<AssetTransactionListView>> GetActiveLoansListAsync();
    
    /// <summary>
    /// Gets overdue loans list (alias for GetOverdueLoansListViewAsync)
    /// </summary>
    /// <returns>Collection of overdue loan list views</returns>
    Task<IEnumerable<AssetTransactionListView>> GetOverdueLoansListAsync();
    
    /// <summary>
    /// Gets available transactions for pairing (LOAN or MAINTENANCE that are not yet paired)
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    /// <param name="pairSourceType">Transaction type to pair with (LOAN or MAINTENANCE)</param>
    /// <returns>Collection of transaction dropdown views</returns>
    Task<IEnumerable<AssetTransactionDropdownView>> GetAvailablePairedTransactionsAsync(int assetId, int pairSourceType);
}