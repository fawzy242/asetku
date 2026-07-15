namespace Whitebird.Domain.Features.AssetTransaction;

/// <summary>
/// View model for asset transaction attachment
/// </summary>
public class AssetTransactionAttachmentView
{
    public int AssetTransactionAttachmentId { get; set; }
    public int AssetTransactionId { get; set; }
    public int FileAttachmentId { get; set; }
    public int Order { get; set; }
    public string? FileName { get; set; }
    public string? OriginalFileName { get; set; }
    public string? FileUrl { get; set; }
    public string? FileCategory { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
}