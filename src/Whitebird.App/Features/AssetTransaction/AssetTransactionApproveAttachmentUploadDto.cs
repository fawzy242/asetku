using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Whitebird.App.Features.AssetTransaction;

/// <summary>
/// DTO for uploading approve attachment (Application layer - with IFormFile)
/// </summary>
public class AssetTransactionApproveAttachmentUploadDto
{
    [Required(ErrorMessage = "File is required")]
    public IFormFile File { get; set; } = default!;

    [Required(ErrorMessage = "DocumentType is required")]
    [Range(1, int.MaxValue, ErrorMessage = "DocumentType must be greater than 0")]
    public int DocumentType { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public int Order { get; set; } = 0;
}