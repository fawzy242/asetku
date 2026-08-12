using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.AssetTransaction;

/// <summary>
/// DTO for asset transaction attachment (Domain layer - no IFormFile)
/// </summary>
public class AssetTransactionAttachmentDto
{
    [StringLength(50, ErrorMessage = "FileCategory cannot exceed 50 characters")]
    public string? FileCategory { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public int Order { get; set; } = 0;
}