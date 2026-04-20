using Whitebird.Domain.Features.Common.Entities;
using Whitebird.Domain.Features.Common.Attributes;

namespace Whitebird.Domain.Features.AssetTransaction.Entities;

public class AssetTransactionEntity : AuditableEntity
{
    public int AssetTransactionId { get; set; }
    public int AssetId { get; set; }
    public string TransactionType { get; set; } = default!;
    public int? FromEmployeeId { get; set; }
    public int? ToEmployeeId { get; set; }
    public int? FromLocationId { get; set; }
    public int? ToLocationId { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public string? Notes { get; set; }
    public string? ConditionBefore { get; set; }
    public string? ConditionAfter { get; set; }
    public string TransactionStatus { get; set; } = "Pending";
    public int? ApprovedBy { get; set; }
    public string? MaintenanceType { get; set; }
    public decimal? MaintenanceCost { get; set; }
    public string? VendorName { get; set; }

    // Navigation properties - marked as NotMapped
    [NotMapped]
    public string? AssetCode { get; set; }

    [NotMapped]
    public string? AssetName { get; set; }

    [NotMapped]
    public string? FromEmployeeName { get; set; }

    [NotMapped]
    public string? ToEmployeeName { get; set; }

    [NotMapped]
    public string? FromLocationName { get; set; }

    [NotMapped]
    public string? ToLocationName { get; set; }

    [NotMapped]
    public string? ApprovedByName { get; set; }
}