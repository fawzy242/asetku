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
    public int? FromEmployeeId { get; set; }
    public string? FromEmployeeName { get; set; }
    public int? ToEmployeeId { get; set; }
    public string? ToEmployeeName { get; set; }
    public int? ToLocationId { get; set; }
    public string? ToLocationName { get; set; }
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