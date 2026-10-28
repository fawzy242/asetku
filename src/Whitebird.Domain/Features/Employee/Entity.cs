using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Employee;

public class EmployeeEntity : AuditableEntity
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Address { get; set; }
    public int? DepartmentId { get; set; }
    public int? Position { get; set; }
    public int? EmploymentStatus { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int? OfficeId { get; set; }
    public DateTime? JoinDate { get; set; }
    public DateTime? ResignDate { get; set; }
}

public class EmployeeAttachmentEntity : AuditableEntity
{
    public int EmployeeAttachmentId { get; set; }
    public int EmployeeId { get; set; }
    public int FileAttachmentId { get; set; }
    public int Order { get; set; }
}

public class EmployeePhotoProfileAttachmentEntity : AuditableEntity
{
    public int EmployeePhotoProfileAttachmentId { get; set; }
    public int EmployeeId { get; set; }
    public int FileAttachmentId { get; set; }
}