namespace Whitebird.Domain.Features.AssetTransaction;

public class AssetTransactionDto
{
    public int AssetTransactionId { get; set; }
    public string? AssetTransactionCode { get; set; }
    public int TransactionType { get; set; }
    public string TransactionTypeName { get; set; } = default!;
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ExpectedLoanReturnDate { get; set; }
    public string? Notes { get; set; }
    public int? Condition { get; set; }
    public string? ConditionName { get; set; }
    public bool? Approved { get; set; }
    public string? ApprovedBy { get; set; }
    public int? FromAssetTransactionId { get; set; }
    public bool? Completed { get; set; }
    public string? CompletedBy { get; set; }
}