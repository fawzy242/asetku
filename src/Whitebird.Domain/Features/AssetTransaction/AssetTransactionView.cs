using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.AssetTransaction;

public class AssetTransactionListViewModel
{
    public int AssetTransactionId { get; set; }
    public string? AssetTransactionCode { get; set; }
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public int TransactionType { get; set; }
    public string TransactionTypeName { get; set; } = default!;
    public string? EmployeeName { get; set; }
    public string? OfficeName { get; set; }
    public DateTime TransactionDate { get; set; }
    public bool? Approved { get; set; }
    public DateTime? ExpectedLoanReturnDate { get; set; }
    public int? FromAssetTransactionId { get; set; }
    public bool IsOverdue { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanReturn { get; set; }
    public bool CanPost { get; set; }
}

public class AssetTransactionDetailViewModel
{
    public int AssetTransactionId { get; set; }
    public string? AssetTransactionCode { get; set; }
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public int TransactionType { get; set; }
    public string TransactionTypeName { get; set; } = default!;
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
    public int? MaintenanceType { get; set; }
    public string? MaintenanceTypeName { get; set; }
    public decimal? MaintenanceCost { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public int? FromAssetTransactionId { get; set; }
}

public class AssetTransactionCreateViewModel
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

    public DateTime? ExpectedLoanReturnDate { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

    public int? Condition { get; set; }

    public int? MaintenanceType { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "MaintenanceCost must be positive")]
    public decimal? MaintenanceCost { get; set; }

    public int? FromAssetTransactionId { get; set; }

    // FIX: Added missing field from AssetTransactionEntity
    public int? MaintenanceEmployeeId { get; set; }
}

public class AssetTransactionUpdateViewModel
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
}

public class BulkApprovalRequest
{
    [Required(ErrorMessage = "Ids are required")]
    public List<int> Ids { get; set; } = new();
}

/// <summary>
/// Request body for the single approval-document action endpoint.
/// The target attachmentId is supplied via the route
/// (POST /api/AssetTransaction/{id}/approve-attachments/{attachmentId}/action),
/// so it is intentionally not part of this DTO.
/// </summary>
public class ApproveAttachmentActionDto
{
    /// <summary>
    /// Supported values: "approve" or "revision" (case-insensitive).
    /// - approve  → sets Approved = 1, NeedToChange = 0
    /// - revision → sets Approved = 0, NeedToChange = 1
    /// </summary>
    [Required(ErrorMessage = "Action is required")]
    public string Action { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}