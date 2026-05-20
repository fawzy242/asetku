using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Department;

public class DepartmentListViewModel
{
    public int DepartmentId { get; set; }
    public string? DepartmentCode { get; set; }
    public string DepartmentName { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int EmployeeCount { get; set; }
}

public class DepartmentDetailViewModel
{
    public int DepartmentId { get; set; }
    public string? DepartmentCode { get; set; }
    public string DepartmentName { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int EmployeeCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

public class DepartmentCreateViewModel
{
    [StringLength(100, ErrorMessage = "DepartmentCode cannot exceed 100 characters")]
    public string? DepartmentCode { get; set; }

    [Required(ErrorMessage = "DepartmentName is required")]
    [StringLength(100, ErrorMessage = "DepartmentName cannot exceed 100 characters")]
    public string DepartmentName { get; set; } = default!;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}

public class DepartmentUpdateViewModel
{
    [StringLength(100, ErrorMessage = "DepartmentCode cannot exceed 100 characters")]
    public string? DepartmentCode { get; set; }

    [Required(ErrorMessage = "DepartmentName is required")]
    [StringLength(100, ErrorMessage = "DepartmentName cannot exceed 100 characters")]
    public string DepartmentName { get; set; } = default!;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "IsActive is required")]
    public bool IsActive { get; set; }
}