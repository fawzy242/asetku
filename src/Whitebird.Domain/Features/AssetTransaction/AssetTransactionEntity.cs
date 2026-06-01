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
    
    // NOTE: Semua NotMapped properties telah dihapus.
    // Untuk query dengan JOIN, gunakan AssetTransactionListView atau AssetTransactionDetailView.
}