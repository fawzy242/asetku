namespace Whitebird.Domain.Features.Department;

/// <summary>
/// View model untuk Department dropdown (minimal data)
/// </summary>
public class DepartmentDropdownView
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? DepartmentCode { get; set; }
}