using System;
using System.Collections.Generic;

namespace Whitebird.Domain.Features.Employee;

/// <summary>
/// View model untuk employee asset summary (response dari GET /api/Employee/{id}/asset-summary)
/// </summary>
public class EmployeeAssetSummaryView
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? EmploymentStatusName { get; set; }

    // Statistics
    public int CurrentlyHeldAssets { get; set; }
    public int AssetsOnLoan { get; set; }
    public int OverdueLoans { get; set; }
    public int TotalHistoricalAssets { get; set; }
    public int ReturnedAssets { get; set; }
    public int DamagedReturns { get; set; }

    // Detailed data
    public List<EmployeeCurrentAssetView> CurrentAssets { get; set; } = new();
    public List<EmployeeAssetHistoryView> AssetHistory { get; set; } = new();
}

/// <summary>
/// View model untuk asset yang sedang dipegang employee
/// </summary>
public class EmployeeCurrentAssetView
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
    public decimal? PurchasePrice { get; set; }
}

/// <summary>
/// View model untuk history asset employee
/// </summary>
public class EmployeeAssetHistoryView
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