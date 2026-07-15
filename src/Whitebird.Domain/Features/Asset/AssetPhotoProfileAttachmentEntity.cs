using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Asset;

public class AssetPhotoProfileAttachmentEntity : AuditableEntity
{
    public int AssetPhotoProfileAttachmentId { get; set; }
    public int AssetId { get; set; }
    public int FileAttachmentId { get; set; }
}