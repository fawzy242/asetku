namespace Whitebird.Domain.Features.UserRoles;

/// <summary>
/// Flat view of a (User, Role) assignment used by the UserRoles grid and per-user
/// endpoints. User and role display names are joined from Users / Roles.
/// </summary>
public class UserRolesListView
{
    public int UserRoleId { get; set; }
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? UserFullName { get; set; }
    public string? UserEmail { get; set; }
    public int RoleId { get; set; }
    public string? RoleCode { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }

    // Action flag
    public bool CanDelete { get; set; }
}