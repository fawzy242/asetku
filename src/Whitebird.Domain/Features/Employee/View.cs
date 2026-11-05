using Whitebird.Domain.Features.FileAttachment;

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

/// <summary>
/// View model for employee detail (response from GetById)
/// </summary>
public class EmployeeDetailView
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

    // ============================================================
    // Photo Profile & Attachments
    // ============================================================
    /// <summary>
    /// Relative URL to the Employee photo endpoint
    /// (e.g. "/api/Employee/1/photo"). Null when no active photo exists.
    /// Populated by EmployeeService via FileResourceUrlResolver — never
    /// constructed here.
    /// </summary>
    public string? PhotoProfileUrl { get; set; }

    /// <summary>
    /// Employee attachments. Each entry's DownloadUrl is populated by
    /// EmployeeService via FileResourceUrlResolver.
    /// </summary>
    public List<EmployeeAttachmentViewDto> Attachments { get; set; } = new();
}

/// <summary>
/// DTO for employee attachment in detail view.
/// Inherits the standardized FileResourceDto shape (FileName,
/// OriginalFileName, FileCategory, ContentType, FileSize, IsPrimary, Order,
/// DownloadUrl, CreatedDate, and the temporary FileUrl alias).
/// </summary>
public class EmployeeAttachmentViewDto : FileResourceDto
{
    public int EmployeeAttachmentId { get; set; }
}

/// <summary>
/// View model untuk Employee dropdown (minimal data)
/// </summary>
public class EmployeeDropdownView
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}