using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Asset;

public class AssetAttachmentEntity : AuditableEntity
{
    public int AssetAttachmentId { get; set; }
    public int AssetId { get; set; }
    public int FileAttachmentId { get; set; }
    public int Order { get; set; }
}