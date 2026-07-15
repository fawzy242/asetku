using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.AssetTransaction;

public class AssetTransactionAttachmentEntity : AuditableEntity
{
    public int AssetTransactionAttachmentId { get; set; }
    public int AssetTransactionId { get; set; }
    public int FileAttachmentId { get; set; }
    public int Order { get; set; }
}