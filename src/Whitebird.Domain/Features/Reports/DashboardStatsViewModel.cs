using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Reports;

/// <summary>
/// Dashboard statistics view model
/// </summary>
public class DashboardStatsViewModel
{
    // Asset counts
    [Display(Name = "Total Assets")]
    public int TotalAssets { get; set; }

    [Display(Name = "Available Assets")]
    public int AvailableAssets { get; set; }

    [Display(Name = "Assigned Assets")]
    public int AssignedAssets { get; set; }

    [Display(Name = "Assets On Loan")]
    public int AssetsOnLoan { get; set; }

    [Display(Name = "Assets In Maintenance")]
    public int AssetsInMaintenance { get; set; }

    [Display(Name = "Disposed Assets")]
    public int DisposedAssets { get; set; }

    [Display(Name = "Damaged Assets")]
    public int DamagedAssets { get; set; }

    // Asset alerts
    [Display(Name = "Expired Warranty")]
    public int ExpiredWarrantyCount { get; set; }

    [Display(Name = "Upcoming Maintenance (30 days)")]
    public int UpcomingMaintenanceCount { get; set; }

    [Display(Name = "Overdue Loans")]
    public int OverdueLoanCount { get; set; }

    // Financial
    [Display(Name = "Total Asset Value")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal TotalAssetValue { get; set; }

    // Employee counts
    [Display(Name = "Total Employees")]
    public int TotalEmployees { get; set; }

    [Display(Name = "Active Employees")]
    public int ActiveEmployees { get; set; }

    // Transaction stats
    [Display(Name = "Pending Approvals")]
    public int PendingApprovals { get; set; }

    [Display(Name = "Approved Transactions (30 days)")]
    public int ApprovedTransactions { get; set; }

    [Display(Name = "Rejected Transactions (30 days)")]
    public int RejectedTransactions { get; set; }

    [Display(Name = "Last 30 Days Transactions")]
    public int Last30DaysTransactions { get; set; }

    // Organization stats
    [Display(Name = "Total Offices")]
    public int TotalOffices { get; set; }

    [Display(Name = "Total Departments")]
    public int TotalDepartments { get; set; }

    // Derived metrics
    [Display(Name = "Asset Utilization Rate")]
    [DisplayFormat(DataFormatString = "{0:F1}%")]
    public decimal AssetUtilizationRate => TotalAssets > 0
        ? (decimal)(AssignedAssets + AssetsOnLoan) / TotalAssets * 100
        : 0;

    [Display(Name = "Asset Availability Rate")]
    [DisplayFormat(DataFormatString = "{0:F1}%")]
    public decimal AssetAvailabilityRate => TotalAssets > 0
        ? (decimal)AvailableAssets / TotalAssets * 100
        : 0;

    [Display(Name = "Average Asset Value")]
    [DisplayFormat(DataFormatString = "{0:C}")]
    public decimal AverageAssetValue => TotalAssets > 0
        ? TotalAssetValue / TotalAssets
        : 0;

    // Recent transactions
    public IEnumerable<RecentTransactionDto> RecentTransactions { get; set; } = new List<RecentTransactionDto>();
}