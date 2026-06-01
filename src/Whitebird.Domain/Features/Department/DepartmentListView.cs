using System;

namespace Whitebird.Domain.Features.Department;

/// <summary>
/// View model untuk Department grid/list (bukan Entity!)
/// </summary>
public class DepartmentListView
{
    public int DepartmentId { get; set; }
    public string? DepartmentCode { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EmployeeCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}