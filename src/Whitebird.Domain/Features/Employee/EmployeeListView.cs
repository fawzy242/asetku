using System;

namespace Whitebird.Domain.Features.Employee;

/// <summary>
/// View model untuk Employee grid/list (bukan Entity!)
/// Data dari JOIN: Department, Office, MasterData (Position, EmploymentStatus)
/// </summary>
public class EmployeeListView
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
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
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }

    // Action flags untuk grid
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}