using System;

namespace Whitebird.Domain.Features.AssetTransaction;

/// <summary>
/// View model untuk AssetTransaction grid/list
/// </summary>
public class AssetTransactionListView
{
    public int AssetTransactionId { get; set; }
    public string? AssetTransactionCode { get; set; }
    public int AssetId { get; set; }
    public string? AssetCode { get; set; }
    public string? AssetName { get; set; }
    public int TransactionType { get; set; }
    public string? TransactionTypeName { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
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
    public int? FromAssetTransactionId { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }

    // Action flags untuk grid
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanReturn { get; set; }
    public bool CanPost { get; set; }

    // ============================================================
    // NEW: Attachments for grid (preview)
    // ============================================================
    public List<AssetTransactionAttachmentInfoDto> Attachments { get; set; } = new();
    public List<AssetTransactionApproveAttachmentInfoDto> ApproveAttachments { get; set; } = new();
}

/// <summary>
/// Attachment info for transaction grid
/// </summary>
public class AssetTransactionAttachmentInfoDto
{
    public int AssetTransactionAttachmentId { get; set; }
    public int FileAttachmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public int Order { get; set; }
}

/// <summary>
/// Approve attachment info for transaction grid
/// </summary>
public class AssetTransactionApproveAttachmentInfoDto
{
    public int AssetTransactionApproveAttachmentId { get; set; }
    public int FileAttachmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public int DocumentType { get; set; }
    public string? DocumentTypeName { get; set; }
    public bool NeedToChange { get; set; }
    public bool Approved { get; set; }
    public int Order { get; set; }
}