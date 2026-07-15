using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Employee;

public class EmployeeAttachmentEntity : AuditableEntity
{
    public int EmployeeAttachmentId { get; set; }
    public int EmployeeId { get; set; }
    public int FileAttachmentId { get; set; }
    public int Order { get; set; }
}