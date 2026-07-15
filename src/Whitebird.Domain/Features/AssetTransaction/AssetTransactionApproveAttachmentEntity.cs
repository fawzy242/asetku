using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.AssetTransaction;

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