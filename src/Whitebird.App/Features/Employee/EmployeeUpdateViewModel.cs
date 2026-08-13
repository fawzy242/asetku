namespace Whitebird.App.Features.Employee;

/// <summary>
/// DTO khusus untuk Update Employee dengan file upload (base64)
/// </summary>
public class EmployeeUpdateFormDto
{
    public string EmployeeCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Address { get; set; }
    public int? DepartmentId { get; set; }
    public int? Position { get; set; }
    public int? EmploymentStatus { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public int? OfficeId { get; set; }
    public DateTime? JoinDate { get; set; }
    public DateTime? ResignDate { get; set; }
    public bool IsActive { get; set; }

    // File upload sebagai base64 (bukan IFormFile)
    public string? PhotoFileBase64 { get; set; }
    public string? PhotoFileName { get; set; }
    public List<string>? AttachmentFilesBase64 { get; set; }
    public List<string>? AttachmentFileNames { get; set; }
    public List<int>? AttachmentIdsToKeep { get; set; }
    public bool ReplaceAttachments { get; set; } = false;
}