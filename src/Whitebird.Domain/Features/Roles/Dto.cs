using System.ComponentModel.DataAnnotations;
using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Roles;

/// <summary>
/// Grid request for Roles. No additional filters beyond the standard search /
/// IsActive / SortBy set.
/// </summary>
public class RolesGridRequest : GridRequestBase
{
}

public class RolesListViewModel
{
    public int RoleId { get; set; }
    public string? RoleCode { get; set; }
    public string RoleName { get; set; } = default!;
    public string? Description { get; set; }
    public int AssignedUserCount { get; set; }
    public bool IsActive { get; set; }
}

public class RolesDetailViewModel
{
    public int RoleId { get; set; }
    public string? RoleCode { get; set; }
    public string RoleName { get; set; } = default!;
    public string? Description { get; set; }
    public int AssignedUserCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

public class RolesCreateViewModel
{
    [StringLength(50, ErrorMessage = "RoleCode cannot exceed 50 characters")]
    public string? RoleCode { get; set; }

    [Required(ErrorMessage = "RoleName is required")]
    [StringLength(100, ErrorMessage = "RoleName cannot exceed 100 characters")]
    public string RoleName { get; set; } = default!;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}

public class RolesUpdateViewModel
{
    [StringLength(50, ErrorMessage = "RoleCode cannot exceed 50 characters")]
    public string? RoleCode { get; set; }

    [Required(ErrorMessage = "RoleName is required")]
    [StringLength(100, ErrorMessage = "RoleName cannot exceed 100 characters")]
    public string RoleName { get; set; } = default!;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "IsActive is required")]
    public bool IsActive { get; set; }
}