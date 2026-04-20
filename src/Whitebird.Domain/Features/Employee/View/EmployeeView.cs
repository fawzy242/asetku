using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Employee.View;

public class EmployeeListViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? Division { get; set; }
    public string? Branch { get; set; }
    public string? Email { get; set; }
    public string EmploymentStatus { get; set; } = "Active";
    public bool IsActive { get; set; }
}

public class EmployeeDetailViewModel
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
    public bool IsActive { get; set; }
    public int ActiveAssetsCount { get; set; }
}

public class EmployeeCreateViewModel
{
    [Required(ErrorMessage = "FullName is required")]
    [StringLength(100, ErrorMessage = "FullName cannot exceed 100 characters")]
    public string FullName { get; set; } = default!;

    [StringLength(50, ErrorMessage = "Department cannot exceed 50 characters")]
    public string? Department { get; set; }

    [StringLength(50, ErrorMessage = "Position cannot exceed 50 characters")]
    public string? Position { get; set; }

    [StringLength(50, ErrorMessage = "Division cannot exceed 50 characters")]
    public string? Division { get; set; }

    [StringLength(50, ErrorMessage = "Branch cannot exceed 50 characters")]
    public string? Branch { get; set; }

    [StringLength(50, ErrorMessage = "CostCenter cannot exceed 50 characters")]
    public string? CostCenter { get; set; }

    [StringLength(20, ErrorMessage = "PhoneNumber cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [StringLength(100, ErrorMessage = "OfficeLocation cannot exceed 100 characters")]
    public string? OfficeLocation { get; set; }

    [StringLength(20, ErrorMessage = "EmploymentStatus cannot exceed 20 characters")]
    public string EmploymentStatus { get; set; } = "Active";

    public DateTime? JoinDate { get; set; }
}

public class EmployeeUpdateViewModel
{
    [Required(ErrorMessage = "FullName is required")]
    [StringLength(100, ErrorMessage = "FullName cannot exceed 100 characters")]
    public string FullName { get; set; } = default!;

    [StringLength(50, ErrorMessage = "Department cannot exceed 50 characters")]
    public string? Department { get; set; }

    [StringLength(50, ErrorMessage = "Position cannot exceed 50 characters")]
    public string? Position { get; set; }

    [StringLength(50, ErrorMessage = "Division cannot exceed 50 characters")]
    public string? Division { get; set; }

    [StringLength(50, ErrorMessage = "Branch cannot exceed 50 characters")]
    public string? Branch { get; set; }

    [StringLength(50, ErrorMessage = "CostCenter cannot exceed 50 characters")]
    public string? CostCenter { get; set; }

    [StringLength(20, ErrorMessage = "PhoneNumber cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [StringLength(100, ErrorMessage = "OfficeLocation cannot exceed 100 characters")]
    public string? OfficeLocation { get; set; }

    [StringLength(20, ErrorMessage = "EmploymentStatus cannot exceed 20 characters")]
    public string EmploymentStatus { get; set; } = "Active";

    public DateTime? JoinDate { get; set; }
    public DateTime? ResignDate { get; set; }

    [Required(ErrorMessage = "IsActive is required")]
    public bool IsActive { get; set; }
}