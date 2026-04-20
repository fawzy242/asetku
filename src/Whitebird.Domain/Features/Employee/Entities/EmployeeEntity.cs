using Whitebird.Domain.Features.Common.Entities;

namespace Whitebird.Domain.Features.Employee.Entities;

public class EmployeeEntity : AuditableEntity
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? Division { get; set; }
    public string? Branch { get; set; }
    public string? CostCenter { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? OfficeLocation { get; set; }
    public string EmploymentStatus { get; set; } = "Active";
    public DateTime? JoinDate { get; set; }
    public DateTime? ResignDate { get; set; }
}