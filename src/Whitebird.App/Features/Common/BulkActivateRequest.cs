using System.ComponentModel.DataAnnotations;

namespace Whitebird.App.Features.Common;

public class BulkActivateRequest
{
    [Required(ErrorMessage = "Ids are required")]
    public List<int> Ids { get; set; } = new();

    [Required(ErrorMessage = "Activate flag is required")]
    public bool Activate { get; set; }
}