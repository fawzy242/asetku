using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Whitebird.Domain.Features.FileAttachment;

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

/// <summary>
/// Upload response returned by <c>IFileAttachmentService.UploadAsync</c>,
/// <c>UploadBytesAsync</c>, and <c>UploadMultipleAsync</c>.
///
/// Inherits the standardized <see cref="FileResourceDto"/> shape (including
/// <c>DownloadUrl</c>, <c>ContentType</c>, <c>FileSize</c>, and the temporary
/// <c>FileUrl</c> alias) and adds <see cref="PreviewUrl"/> for image previews.
///
/// URLs are RELATIVE (produced by <see cref="FileResourceUrlResolver"/>); the
/// frontend prepends its configured backend base URL.
/// </summary>
public class FileAttachmentResponseDto : FileResourceDto
{
    /// <summary>
    /// Preview URL for images (same value as <see cref="FileResourceDto.DownloadUrl"/>
    /// for image files, null otherwise).
    /// </summary>
    public string? PreviewUrl { get; set; }
}