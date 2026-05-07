namespace Whitebird.Domain.Features.Employee.View;

/// <summary>
/// Comprehensive summary of employee's asset interactions
/// </summary>
public class EmployeeAssetSummaryViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Department { get; set; }
    public string EmploymentStatus { get; set; } = default!;

    public int CurrentlyHeldAssets { get; set; }
    public int AssetsOnLoan { get; set; }
    public int OverdueLoans { get; set; }
    public int TotalHistoricalAssets { get; set; }
    public int ReturnedAssets { get; set; }
    public int DamagedReturns { get; set; }

    public List<EmployeeAssetDetail> CurrentAssets { get; set; } = new();
    public List<EmployeeAssetHistory> AssetHistory { get; set; } = new();
}

public class EmployeeAssetDetail
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string AssociationType { get; set; } = default!; // "Assigned", "On Loan"
    public DateTime SinceDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public bool IsOverdue { get; set; }
    public string? Condition { get; set; }
}

public class EmployeeAssetHistory
{
    public int AssetTransactionId { get; set; }
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public string TransactionType { get; set; } = default!;
    public DateTime TransactionDate { get; set; }
    public string? FromEmployeeName { get; set; }
    public string? ToEmployeeName { get; set; }
    public string? ConditionAfter { get; set; }
    public string? Notes { get; set; }
}