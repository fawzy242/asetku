using Whitebird.Domain.Features.Common.Entities;
using Whitebird.Domain.Features.Common.Attributes;

namespace Whitebird.Domain.Features.Location.Entities;

public class LocationEntity : AuditableEntity
{
    public int LocationId { get; set; }
    public string LocationCode { get; set; } = default!;
    public string LocationName { get; set; } = default!;
    public string? LocationType { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public int? ParentLocationId { get; set; }

    // Navigation properties - marked as NotMapped
    [NotMapped]
    public string? ParentLocationName { get; set; }

    [NotMapped]
    public int ChildCount { get; set; }
}