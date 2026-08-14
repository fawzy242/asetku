namespace Whitebird.Domain.Features.Employee;

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
    // NEW: Photo Profile & Attachments
    // ============================================================
    public string? PhotoProfileUrl { get; set; }
    public List<EmployeeAttachmentViewDto> Attachments { get; set; } = new();
}

/// <summary>
/// DTO for employee attachment in detail view
/// </summary>
public class EmployeeAttachmentViewDto
{
    public int EmployeeAttachmentId { get; set; }
    public int FileAttachmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public string? FileCategory { get; set; }
    public bool IsPrimary { get; set; }
    public int Order { get; set; }
    public DateTime CreatedDate { get; set; }
}