using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.AssetTransaction;

public class AssetTransactionEntity : AuditableEntity
{
    public int AssetTransactionId { get; set; }
    public string? Notes { get; set; }
    public bool? Approved { get; set; }
    public string? ApprovedBy { get; set; }
    public bool? Completed { get; set; }
    public string? CompletedBy { get; set; }
    public string? AssetTransactionCode { get; set; }
    public DateTime TransactionDate { get; set; }
    public int TransactionType { get; set; }
    public int AssetId { get; set; }
    public int? EmployeeId { get; set; }
    public int? OfficeId { get; set; }
    public int? Condition { get; set; }
    public int? FromAssetTransactionId { get; set; }
    public DateTime? ExpectedLoanReturnDate { get; set; }
    public int? MaintenanceEmployeeId { get; set; }
    public int? MaintenanceType { get; set; }
    public decimal? MaintenanceCost { get; set; }
}