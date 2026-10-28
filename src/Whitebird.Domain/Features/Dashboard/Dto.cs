using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Dashboard;

/// <summary>
/// Grid request for Dashboard detail stats (drilldown)
/// </summary>
public class DashboardDetailStatsRequest : GridRequestBase
{
    /// <summary>
    /// Type of detail stats: Available, Assigned, OnLoan, InMaintenance, Disposed, Damaged, ExpiredWarranty, OverdueLoans
    /// </summary>
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Response model for detail stats (drilldown)
/// </summary>
public class DetailStatsResponse
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public PaginatedResult<object> Data { get; set; } = new();
}

/// <summary>
/// DTO for asset detail in drilldown grid
/// </summary>
public class AssetDetailDto
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? CurrentStatus { get; set; }
    public string? CurrentHolderName { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for category breakdown on dashboard
/// </summary>
public class CategoryBreakdownDto
{
    public string Category { get; set; } = string.Empty;
    public int AssetCount { get; set; }
    public decimal TotalValue { get; set; }
}

/// <summary>
/// DTO for expired warranty assets
/// </summary>
public class ExpiredWarrantyDto
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    public string? SupplierName { get; set; }
    public string? CurrentStatus { get; set; }
}

/// <summary>
/// DTO for monthly statistics on dashboard
/// </summary>
public class MonthlyStatDto
{
    public int Month { get; set; }
    public int TransactionCount { get; set; }
    public int UniqueAssetsCount { get; set; }
}

/// <summary>
/// DTO for overdue loan detail in drilldown grid
/// </summary>
public class OverdueLoanDetailDto
{
    public int AssetTransactionId { get; set; }
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public DateTime ExpectedLoanReturnDate { get; set; }
    public int DaysOverdue { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for recent transactions on dashboard
/// </summary>
public class RecentTransactionDto
{
    public int AssetTransactionId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public int TransactionType { get; set; }
    public string TransactionTypeName { get; set; } = string.Empty;
    public string? FromEmployeeName { get; set; }
    public string? ToEmployeeName { get; set; }
    public DateTime TransactionDate { get; set; }
    public bool? Approved { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for schedule statistics on dashboard
/// </summary>
public class ScheduleStatsDto
{
    public int TotalHandover { get; set; }
    public int TotalReturn { get; set; }
    public int TotalMaintenance { get; set; }
    public int UpcomingHandover { get; set; }
    public int UpcomingReturn { get; set; }
    public int UpcomingMaintenance { get; set; }
}