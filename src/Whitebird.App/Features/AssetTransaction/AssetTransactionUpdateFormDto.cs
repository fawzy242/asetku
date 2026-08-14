using System.ComponentModel.DataAnnotations;

namespace Whitebird.App.Features.AssetTransaction;

/// <summary>
/// DTO khusus untuk Update Asset Transaction dengan file upload (base64)
/// Untuk data dari grid standar (GetGridAdd / GetGridReturn)
/// </summary>
public class AssetTransactionUpdateFormDto
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

    public bool IsActive { get; set; }

    // ============================================================
    // File upload sebagai base64
    // ============================================================
    public List<string>? AttachmentFilesBase64 { get; set; }
    public List<string>? AttachmentFileNames { get; set; }
    public List<int>? AttachmentIdsToKeep { get; set; }
    public bool ReplaceAttachments { get; set; } = false;

    // Approve Attachments
    public List<ApproveAttachmentBase64Dto>? ApproveAttachments { get; set; }
    public List<int>? ApproveAttachmentIdsToKeep { get; set; }
    public bool ReplaceApproveAttachments { get; set; } = false;
}

/// <summary>
/// DTO for approve attachment with base64
/// </summary>
public class ApproveAttachmentBase64Dto
{
    [Required(ErrorMessage = "DocumentType is required")]
    [Range(1, int.MaxValue, ErrorMessage = "DocumentType must be greater than 0")]
    public int DocumentType { get; set; }

    public string FileBase64 { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int Order { get; set; } = 0;
}