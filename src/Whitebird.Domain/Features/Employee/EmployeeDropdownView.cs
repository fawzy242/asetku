namespace Whitebird.Domain.Features.Employee;

/// <summary>
/// View model untuk employee dropdown (minimal data)
/// </summary>
public class EmployeeDropdownView
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}