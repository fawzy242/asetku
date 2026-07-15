namespace Whitebird.Domain.Features.AssetTransaction;

/// <summary>
/// View model for asset transaction approve attachment
/// </summary>
public class AssetTransactionApproveAttachmentView
{
    public int AssetTransactionApproveAttachmentId { get; set; }
    public int AssetTransactionId { get; set; }
    public int FileAttachmentId { get; set; }
    public int Order { get; set; }
    public int DocumentType { get; set; }
    public string? DocumentTypeName { get; set; }
    public bool IsRequired { get; set; }
    public bool NeedToChange { get; set; }
    public bool Approved { get; set; }
    public string? FileName { get; set; }
    public string? OriginalFileName { get; set; }
    public string? FileUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
}