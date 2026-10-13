using System.ComponentModel.DataAnnotations;

namespace Whitebird.App.Features.Common;

/// <summary>
/// Request model for bulk delete operations (per-menu endpoints).
/// Mirrors <see cref="BulkActivateRequest"/> so that the request shape stays
/// consistent across all bulk operations in the project.
/// </summary>
public class BulkDeleteRequest
{
    /// <summary>
    /// IDs of the records to delete. Must contain at least one ID.
    /// </summary>
    [Required(ErrorMessage = "Ids are required")]
    [MinLength(1, ErrorMessage = "At least one ID is required")]
    public List<int> Ids { get; set; } = new();
}