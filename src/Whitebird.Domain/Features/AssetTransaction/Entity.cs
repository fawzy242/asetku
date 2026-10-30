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

    /// <summary>
    /// P3-7 — Pairing invariant.
    ///
    /// When a transaction is paired with a companion (RETURN, LOAN_RETURN,
    /// POST_MAINTENANCE), <c>FromAssetTransactionId</c> is written on BOTH rows:
    ///   * The child row (RETURN / LOAN_RETURN / POST_MAINTENANCE) points at the
    ///     source transaction it closes: <c>child.FromAssetTransactionId = source.Id</c>.
    ///   * The source row (HANDOVER / LOAN / MAINTENANCE / OPERATIONAL_OFFICE /
    ///     TRANSFER) is updated to point at the child that was just created:
    ///     <c>source.FromAssetTransactionId = child.Id</c>.
    ///
    /// "Active" transaction semantics (used everywhere status is derived) treat
    /// only rows with <c>FromAssetTransactionId IS NULL AND Approved = 1
    /// AND IsActive = 1</c> as current. Once a source is paired, its
    /// <c>FromAssetTransactionId</c> is no longer null, so it correctly drops
    /// out of the "active" set — the asset is then considered free / returned /
    /// out of maintenance depending on the child's type.
    ///
    /// Delete / soft-delete / deactivate guards in AssetTransactionService rely
    /// on this invariant: any transaction with a paired child (found via
    /// GetPairedTransactionAsync, which queries "rows where
    /// FromAssetTransactionId = @Id") cannot be removed or deactivated because
    /// doing so would orphan the child.
    /// </summary>
    public int? FromAssetTransactionId { get; set; }

    public DateTime? ExpectedLoanReturnDate { get; set; }
    public int? MaintenanceEmployeeId { get; set; }
    public int? MaintenanceType { get; set; }
    public decimal? MaintenanceCost { get; set; }
}

public class AssetTransactionAttachmentEntity : AuditableEntity
{
    public int AssetTransactionAttachmentId { get; set; }
    public int AssetTransactionId { get; set; }
    public int FileAttachmentId { get; set; }
    public int Order { get; set; }
}

public class AssetTransactionApproveAttachmentEntity : AuditableEntity
{
    public int AssetTransactionApproveAttachmentId { get; set; }
    public int AssetTransactionId { get; set; }
    public int FileAttachmentId { get; set; }
    public int Order { get; set; }
    public int DocumentType { get; set; }
    public bool NeedToChange { get; set; }
    public bool Approved { get; set; }
}