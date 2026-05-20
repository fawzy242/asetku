namespace Whitebird.App.Features.Employee;

public class EmployeeImportDto
{
    public string EmployeeCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Address { get; set; }
    public string? DepartmentName { get; set; }
    public int? DepartmentId { get; set; }
    public string? PositionName { get; set; }
    public int? Position { get; set; }
    public string? EmploymentStatusName { get; set; }
    public int? EmploymentStatus { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? OfficeName { get; set; }
    public int? OfficeId { get; set; }
    public DateTime? JoinDate { get; set; }
    public DateTime? ResignDate { get; set; }
}