namespace Whitebird.Domain.Features.AssetTransaction;

/// <summary>
/// Request model for asset transaction grid (unified for all grids)
/// </summary>
public class AssetTransactionGridRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? EmployeeId { get; set; }
    public int? AssetId { get; set; }
    public int? TransactionType { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;

    // ============================================================
    // NEW: Status filter for grid
    // ============================================================
    /// <summary>
    /// Filter by status: Approve, Pending, Reject
    /// - Approve: IsActive = 1 AND Approved = 1
    /// - Pending: IsActive = 1 AND Approved IS NULL
    /// - Reject: IsActive = 0 AND Approved = 0
    /// </summary>
    public string? Status { get; set; }
}