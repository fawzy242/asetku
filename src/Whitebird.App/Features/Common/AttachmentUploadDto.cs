using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Whitebird.App.Features.Common;

/// <summary>
/// Shared DTO for uploading an entity attachment (Asset, Employee).
/// Used by the standalone attachment endpoints:
///   POST /api/{Asset|Employee}/{id}/attachments
/// </summary>
public class AttachmentUploadDto
{
    [Required(ErrorMessage = "File is required")]
    public IFormFile File { get; set; } = default!;

    [StringLength(50, ErrorMessage = "FileCategory cannot exceed 50 characters")]
    public string? FileCategory { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public int Order { get; set; } = 0;
}