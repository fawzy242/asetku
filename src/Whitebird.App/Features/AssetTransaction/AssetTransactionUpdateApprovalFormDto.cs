using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Whitebird.Domain.Features.Common;

namespace Whitebird.App.Features.AssetTransaction;

/// <summary>
/// DTO khusus untuk Update Asset Transaction Approval dengan file upload (base64)
/// Untuk data dari grid pending approval
/// </summary>
public class AssetTransactionUpdateApprovalFormDto
{
    [Required(ErrorMessage = "AssetId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "AssetId must be greater than 0")]
    public int AssetId { get; set; }

    [Required(ErrorMessage = "TransactionType is required")]
    public int TransactionType { get; set; }

    public int? EmployeeId { get; set; }
    public int? OfficeId { get; set; }

    [Required(ErrorMessage = "TransactionDate is required")]
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Required for LOAN; unused for HANDOVER / TRANSFER / MAINTENANCE /
    /// DISPOSAL / OPERATIONAL_OFFICE. Tolerates "" as null so a form that
    /// renders this optional field for all types deserializes cleanly
    /// regardless of the chosen TransactionType.
    /// </summary>
    [JsonConverter(typeof(NullableDateTimeConverter))]
    public DateTime? ExpectedLoanReturnDate { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

    public int? Condition { get; set; }

    public bool? Approved { get; set; }
    public string? ApprovedBy { get; set; }
    public bool? Completed { get; set; }
    public string? CompletedBy { get; set; }

    public int? MaintenanceType { get; set; }
    public decimal? MaintenanceCost { get; set; }

    public bool IsActive { get; set; }

    // ============================================================
    // Approve Attachments (base64)
    // ============================================================
    public List<ApproveAttachmentBase64Dto>? ApproveAttachments { get; set; }
    public List<int>? ApproveAttachmentIdsToKeep { get; set; }
    public bool ReplaceApproveAttachments { get; set; } = false;
}