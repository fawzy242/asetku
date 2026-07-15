namespace Whitebird.Domain.Features.AssetTransaction;

/// <summary>
/// View model for asset transaction detail (response from GetById)
/// </summary>
public class AssetTransactionDetailView
{
    public int AssetTransactionId { get; set; }
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public int TransactionType { get; set; }
    public string TransactionTypeName { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeCode { get; set; }
    public int? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ExpectedLoanReturnDate { get; set; }
    public string? Notes { get; set; }
    public int? Condition { get; set; }
    public string? ConditionName { get; set; }
    public bool? Approved { get; set; }
    public string? ApprovedBy { get; set; }
    public bool? Completed { get; set; }
    public string? CompletedBy { get; set; }
    public string? AssetTransactionCode { get; set; }
    public int? MaintenanceType { get; set; }
    public string? MaintenanceTypeName { get; set; }
    public decimal? MaintenanceCost { get; set; }
    public int? FromAssetTransactionId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}