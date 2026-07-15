using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Users;

public class UsersPhotoProfileAttachmentEntity : AuditableEntity
{
    public int UsersPhotoProfileAttachmentId { get; set; }
    public int UserId { get; set; }
    public int FileAttachmentId { get; set; }
}