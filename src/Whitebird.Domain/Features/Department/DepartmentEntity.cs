using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Department;

public class DepartmentEntity : AuditableEntity
{
    public int DepartmentId { get; set; }
    public string? DepartmentCode { get; set; }
    public string DepartmentName { get; set; } = default!;
    public string? Description { get; set; }
}