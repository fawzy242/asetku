using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.AssetTransaction;

/// <summary>
/// View model for POST_MAINTENANCE shortcut operation
/// </summary>
public class PostMaintenanceViewModel
{
    /// <summary>
    /// Date when maintenance was completed
    /// </summary>
    [Required(ErrorMessage = "Completion date is required")]
    public DateTime CompletionDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Asset condition after maintenance
    /// </summary>
    [Range(1, 3, ErrorMessage = "Condition after must be between 1 and 3")]
    public int? ConditionAfter { get; set; }

    /// <summary>
    /// Additional notes about the maintenance
    /// </summary>
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}