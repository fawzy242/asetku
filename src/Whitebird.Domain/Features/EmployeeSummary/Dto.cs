using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.EmployeeSummary;

/// <summary>
/// DTO for employee lookup/dropdown in Employee Summary menu
/// </summary>
public class EmployeeSummaryGetDataDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public bool IsActive { get; set; }

    // P3-4: additional fields folded into the primary employee lookup query so
    // GetSummaryAsync no longer needs four extra round-trips. These values are
    // populated by the JOINed SQL in GetEmployeeDataAsync.
    public string? PositionName { get; set; }
    public string? EmploymentStatusName { get; set; }
    public DateTime? JoinDate { get; set; }
    public DateTime? ResignDate { get; set; }
}

/// <summary>
/// Grid request for Employee Summary
/// </summary>
public class EmployeeSummaryGridRequest : GridRequestBase
{
    /// <summary>
    /// Employee ID (required, passed in path)
    /// </summary>
    public int EmployeeId { get; set; }
}

/// <summary>
/// Request model for employee summary grids
/// </summary>
public class EmployeeSummaryRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}

/// <summary>
/// Employee statistics (cards)
/// </summary>
public class EmployeeSummaryStatsDto
{
    public int CurrentlyHeldAssets { get; set; }
    public int AssetsOnLoan { get; set; }
    public int OverdueLoans { get; set; }
    public int TotalHistoricalAssets { get; set; }
    public int ReturnedAssets { get; set; }
    public int DamagedReturns { get; set; }
}

/// <summary>
/// Current asset holding item (grid)
/// </summary>
public class EmployeeCurrentHoldingDto
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AssociationType { get; set; } = string.Empty;
    public DateTime SinceDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public bool IsOverdue { get; set; }
    public string? ConditionName { get; set; }
}

/// <summary>
/// Assets that were previously held but now held by another employee (grid)
/// </summary>
public class EmployeeAssetsHistoryDto
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime LastHeldDate { get; set; }
    public string? CurrentHolderName { get; set; }
    public int? CurrentHolderId { get; set; }
    public string? ConditionName { get; set; }
}

/// <summary>
/// Employee transaction history item (grid)
/// </summary>
public class EmployeeTransactionHistoryDto
{
    public int AssetTransactionId { get; set; }
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string TransactionTypeName { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string? FromEmployeeName { get; set; }
    public string? ToEmployeeName { get; set; }
    public string? ConditionAfterName { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Transaction stats for transaction history chart
/// </summary>
public class EmployeeTransactionStatsDto
{
    public string TransactionType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Response for transaction stats
/// </summary>
public class EmployeeTransactionStatsResponse
{
    public int TotalTransactions { get; set; }
    public List<EmployeeTransactionStatsDto> Data { get; set; } = new();
}

/// <summary>
/// Category stats for current holding chart
/// </summary>
public class EmployeeCategoryStatsDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Response for category stats
/// </summary>
public class EmployeeCategoryStatsResponse
{
    public int TotalAssets { get; set; }
    public List<EmployeeCategoryStatsDto> Data { get; set; } = new();
}

/// <summary>
/// Schedule stats for employee (handover and return)
/// </summary>
public class EmployeeScheduleStatsDto
{
    public int TotalHandover { get; set; }
    public int TotalReturn { get; set; }
    public int UpcomingHandover { get; set; }
    public int UpcomingReturn { get; set; }
    public List<EmployeeScheduleItemDto> HandoverList { get; set; } = new();
    public List<EmployeeScheduleItemDto> ReturnList { get; set; } = new();
}

/// <summary>
/// Schedule item
/// </summary>
public class EmployeeScheduleItemDto
{
    public int ScheduleId { get; set; }
    public DateTime ScheduleDate { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}