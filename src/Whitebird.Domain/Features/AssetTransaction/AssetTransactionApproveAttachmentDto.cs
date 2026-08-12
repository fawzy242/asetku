using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.AssetTransaction;

/// <summary>
/// DTO for asset transaction approve attachment (Domain layer - no IFormFile)
/// </summary>
public class AssetTransactionApproveAttachmentDto
{
    [Required(ErrorMessage = "DocumentType is required")]
    [Range(1, int.MaxValue, ErrorMessage = "DocumentType must be greater than 0")]
    public int DocumentType { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public int Order { get; set; } = 0;
}