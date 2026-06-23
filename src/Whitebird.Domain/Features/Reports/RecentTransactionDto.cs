namespace Whitebird.Domain.Features.Reports;

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