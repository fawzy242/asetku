namespace Whitebird.Domain.Features.AssetTransaction;

public class AssetTransactionDto
{
    public int AssetTransactionId { get; set; }
    public int TransactionType { get; set; }
    public string TransactionTypeName { get; set; } = default!;
    public int? FromEmployeeId { get; set; }
    public string? FromEmployeeName { get; set; }
    public int? ToEmployeeId { get; set; }
    public string? ToEmployeeName { get; set; }
    public int? ToOfficeId { get; set; }
    public string? ToOfficeName { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public string? Notes { get; set; }
    public int? ConditionBefore { get; set; }
    public string? ConditionBeforeName { get; set; }
    public int? ConditionAfter { get; set; }
    public string? ConditionAfterName { get; set; }
    public bool? Approved { get; set; }
    public string? ApprovedBy { get; set; }
    public int? FromAssetTransactionId { get; set; }
}