using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Whitebird.App.Features.FileAttachment;

public class FileAttachmentUploadViewModel
{
    [Required(ErrorMessage = "File is required")]
    public IFormFile File { get; set; } = default!;

    [StringLength(50, ErrorMessage = "FileCategory cannot exceed 50 characters")]
    public string? FileCategory { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public bool IsPrimary { get; set; } = false;
}

public class FileAttachmentMultipleUploadViewModel
{
    [Required(ErrorMessage = "Files are required")]
    public List<IFormFile> Files { get; set; } = new();

    [StringLength(50, ErrorMessage = "FileCategory cannot exceed 50 characters")]
    public string? FileCategory { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}

public class FileAttachmentUpdateViewModel
{
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public bool IsPrimary { get; set; }
}

public class FileAttachmentResponseDto
{
    public int FileAttachmentId { get; set; }
    public string FileName { get; set; } = default!;
    public string OriginalFileName { get; set; } = default!;
    public string? FileUrl { get; set; }
    public string? PreviewUrl { get; set; }
    public long? FileSize { get; set; }
    public string? FileCategory { get; set; }
    public bool IsPrimary { get; set; }
}