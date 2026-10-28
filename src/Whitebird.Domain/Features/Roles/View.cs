namespace Whitebird.Domain.Features.Roles;

/// <summary>
/// View model for the Roles grid. AssignedUserCount is populated via a correlated
/// subquery against UserRoles (only active assignments counted). CanEdit / CanDelete
/// are computed by the repository.
/// </summary>
public class RolesListView
{
    public int RoleId { get; set; }
    public string? RoleCode { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AssignedUserCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }

    // Action flags
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

public class RolesDetailView
{
    public int RoleId { get; set; }
    public string? RoleCode { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AssignedUserCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}

public class RolesDropdownView
{
    public int RoleId { get; set; }
    public string? RoleCode { get; set; }
    public string RoleName { get; set; } = string.Empty;
}