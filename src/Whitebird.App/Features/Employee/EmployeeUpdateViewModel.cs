using System.ComponentModel.DataAnnotations;

namespace Whitebird.App.Features.Employee;

/// <summary>
/// DTO khusus untuk Update Employee dengan file upload (base64)
/// </summary>
public class EmployeeUpdateFormDto
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

    // File upload sebagai base64 (bukan IFormFile)
    public string? PhotoFileBase64 { get; set; }
    public string? PhotoFileName { get; set; }
    public List<string>? AttachmentFilesBase64 { get; set; }
    public List<string>? AttachmentFileNames { get; set; }
    public List<int>? AttachmentIdsToKeep { get; set; }
    public bool ReplaceAttachments { get; set; } = false;
}