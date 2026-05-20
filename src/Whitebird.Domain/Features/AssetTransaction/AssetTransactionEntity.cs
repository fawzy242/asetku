using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.AssetTransaction;

public class AssetTransactionEntity : AuditableEntity
{
    public int AssetTransactionId { get; set; }
    public int AssetId { get; set; }
    public int TransactionType { get; set; }
    public int? FromEmployeeId { get; set; }
    public int? ToEmployeeId { get; set; }
    public int? ToLocationId { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public string? Notes { get; set; }
    public int? ConditionBefore { get; set; }
    public int? ConditionAfter { get; set; }
    public bool? Approved { get; set; }
    public string? ApprovedBy { get; set; }
    public int? MaintenanceType { get; set; }
    public decimal? MaintenanceCost { get; set; }
    public int? FromAssetTransactionId { get; set; }

    [NotMapped]
    public string? AssetCode { get; set; }

    [NotMapped]
    public string? AssetName { get; set; }

    [NotMapped]
    public string? FromEmployeeName { get; set; }

    [NotMapped]
    public string? ToEmployeeName { get; set; }

    [NotMapped]
    public string? ToLocationName { get; set; }

    [NotMapped]
    public string? TransactionTypeName { get; set; }

    [NotMapped]
    public string? ConditionBeforeName { get; set; }

    [NotMapped]
    public string? ConditionAfterName { get; set; }

    [NotMapped]
    public string? MaintenanceTypeName { get; set; }
}