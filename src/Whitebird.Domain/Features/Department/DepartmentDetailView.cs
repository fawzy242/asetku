namespace Whitebird.Domain.Features.Department;

/// <summary>
/// View model for department detail (response from GetById)
/// </summary>
public class DepartmentDetailView
{
    public int DepartmentId { get; set; }
    public string? DepartmentCode { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int EmployeeCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}