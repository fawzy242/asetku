using System.ComponentModel.DataAnnotations;
using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Employee;

/// <summary>
/// Grid request for Employee
/// </summary>
public class EmployeeGridRequest : GridRequestBase
{
    /// <summary>
    /// Filter by DepartmentId
    /// </summary>
    public int? DepartmentId { get; set; }

    /// <summary>
    /// Filter by OfficeId
    /// </summary>
    public int? OfficeId { get; set; }

    /// <summary>
    /// Filter by EmploymentStatus
    /// </summary>
    public int? EmploymentStatus { get; set; }
}

public class EmployeeListViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public string? EmploymentStatusName { get; set; }
    public string? Email { get; set; }
    public string? OfficeName { get; set; }
    public bool IsActive { get; set; }
}

public class EmployeeDetailViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Address { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? Position { get; set; }
    public string? PositionName { get; set; }
    public int? EmploymentStatus { get; set; }
    public string? EmploymentStatusName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public DateTime? JoinDate { get; set; }
    public DateTime? ResignDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

public class EmployeeCreateViewModel
{
    [Required(ErrorMessage = "EmployeeCode is required")]
    [StringLength(50, ErrorMessage = "EmployeeCode cannot exceed 50 characters")]
    public string EmployeeCode { get; set; } = default!;

    [Required(ErrorMessage = "FullName is required")]
    [StringLength(100, ErrorMessage = "FullName cannot exceed 100 characters")]
    public string FullName { get; set; } = default!;

    [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters")]
    public string? Address { get; set; }

    public int? DepartmentId { get; set; }
    public int? Position { get; set; }
    public int? EmploymentStatus { get; set; }

    [StringLength(20, ErrorMessage = "PhoneNumber cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    public int? OfficeId { get; set; }

    public DateTime? JoinDate { get; set; }

    // FIX: Added missing field from EmployeeEntity
    public DateTime? ResignDate { get; set; }
}

public class EmployeeUpdateViewModel
{
    [Required(ErrorMessage = "EmployeeCode is required")]
    [StringLength(50, ErrorMessage = "EmployeeCode cannot exceed 50 characters")]
    public string EmployeeCode { get; set; } = default!;

    [Required(ErrorMessage = "FullName is required")]
    [StringLength(100, ErrorMessage = "FullName cannot exceed 100 characters")]
    public string FullName { get; set; } = default!;

    [StringLength(300, ErrorMessage = "Address cannot exceed 300 characters")]
    public string? Address { get; set; }

    public int? DepartmentId { get; set; }
    public int? Position { get; set; }
    public int? EmploymentStatus { get; set; }

    [StringLength(20, ErrorMessage = "PhoneNumber cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    public int? OfficeId { get; set; }

    public DateTime? JoinDate { get; set; }
    public DateTime? ResignDate { get; set; }

    [Required(ErrorMessage = "IsActive is required")]
    public bool IsActive { get; set; }
}