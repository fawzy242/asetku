namespace Whitebird.Domain.Features.EmployeeSummary;

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