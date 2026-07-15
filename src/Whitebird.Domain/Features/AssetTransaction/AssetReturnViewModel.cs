using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.AssetTransaction;

public class AssetReturnViewModel
{
    [Required(ErrorMessage = "ActualReturnDate is required")]
    public DateTime ActualReturnDate { get; set; }

    public int? Condition { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}