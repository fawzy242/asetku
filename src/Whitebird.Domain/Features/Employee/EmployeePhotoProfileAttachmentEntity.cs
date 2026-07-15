using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Employee;

public class EmployeePhotoProfileAttachmentEntity : AuditableEntity
{
    public int EmployeePhotoProfileAttachmentId { get; set; }
    public int EmployeeId { get; set; }
    public int FileAttachmentId { get; set; }
}