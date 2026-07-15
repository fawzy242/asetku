namespace Whitebird.Domain.Features.AssetTransaction;

/// <summary>
/// View model for pending approval grid
/// </summary>
public class AssetTransactionPendingApprovalView
{
    public int AssetTransactionId { get; set; }
    public string? AssetTransactionCode { get; set; }
    public string? AssetCode { get; set; }
    public string? AssetName { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? TransactionTypeName { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? ConditionName { get; set; }
    public bool? Approved { get; set; }
    public bool? Completed { get; set; }
    public bool AllAttachmentsApproved { get; set; }
    public bool HasNeedToChange { get; set; }
    public List<ApproveAttachmentStatusDto> Attachments { get; set; } = new();
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanReturn { get; set; }
    public bool CanPost { get; set; }
}

/// <summary>
/// DTO for approve attachment status
/// </summary>
public class ApproveAttachmentStatusDto
{
    public int AttachmentId { get; set; }
    public int DocumentType { get; set; }
    public string? DocumentTypeName { get; set; }
    public bool IsRequired { get; set; }
    public bool IsUploaded { get; set; }
    public bool NeedToChange { get; set; }
    public bool Approved { get; set; }
    public string? FileName { get; set; }
}