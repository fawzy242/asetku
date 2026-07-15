namespace Whitebird.Domain.Features.Dashboard;

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