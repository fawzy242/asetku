using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Whitebird.App.Features.AssetTransaction;

/// <summary>
/// DTO for uploading asset transaction attachment (Application layer - with IFormFile)
/// </summary>
public class AssetTransactionAttachmentUploadDto
{
    [Required(ErrorMessage = "File is required")]
    public IFormFile File { get; set; } = default!;

    [StringLength(50, ErrorMessage = "FileCategory cannot exceed 50 characters")]
    public string? FileCategory { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public int Order { get; set; } = 0;
}