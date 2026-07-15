namespace Whitebird.Domain.Features.EmployeeSummary;

/// <summary>
/// DTO for employee lookup/dropdown in Employee Summary menu
/// </summary>
public class EmployeeSummaryGetDataDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public bool IsActive { get; set; }
}