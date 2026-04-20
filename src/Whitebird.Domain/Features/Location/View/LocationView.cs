using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.Location.View;

public class LocationListViewModel
{
    public int LocationId { get; set; }
    public string LocationCode { get; set; } = default!;
    public string LocationName { get; set; } = default!;
    public string? LocationType { get; set; }
    public string? City { get; set; }
    public int? ParentLocationId { get; set; }
    public string? ParentLocationName { get; set; }
    public bool IsActive { get; set; }
}

public class LocationDetailViewModel
{
    public int LocationId { get; set; }
    public string LocationCode { get; set; } = default!;
    public string LocationName { get; set; } = default!;
    public string? LocationType { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public int? ParentLocationId { get; set; }
    public string? ParentLocationName { get; set; }
    public int ChildCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
}

public class LocationCreateViewModel
{
    [Required(ErrorMessage = "LocationName is required")]
    [StringLength(100, ErrorMessage = "LocationName cannot exceed 100 characters")]
    public string LocationName { get; set; } = default!;

    [StringLength(50, ErrorMessage = "LocationType cannot exceed 50 characters")]
    public string? LocationType { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string? City { get; set; }

    public int? ParentLocationId { get; set; }
}

public class LocationUpdateViewModel
{
    [Required(ErrorMessage = "LocationName is required")]
    [StringLength(100, ErrorMessage = "LocationName cannot exceed 100 characters")]
    public string LocationName { get; set; } = default!;

    [StringLength(50, ErrorMessage = "LocationType cannot exceed 50 characters")]
    public string? LocationType { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string? City { get; set; }

    public int? ParentLocationId { get; set; }

    [Required(ErrorMessage = "IsActive is required")]
    public bool IsActive { get; set; }
}