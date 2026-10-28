using System.ComponentModel.DataAnnotations;
using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.UserRoles;

/// <summary>
/// Grid request for UserRoles. Optional filters narrow the flat grid to a specific
/// user and/or role.
/// </summary>
public class UserRolesGridRequest : GridRequestBase
{
    public int? UserId { get; set; }
    public int? RoleId { get; set; }
}

public class UserRolesAssignRequest
{
    [Required(ErrorMessage = "UserId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than 0")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "RoleId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "RoleId must be greater than 0")]
    public int RoleId { get; set; }
}

public class UserRolesReplaceRequest
{
    /// <summary>
    /// Complete set of role ids the user should have after the operation.
    /// An empty list removes all roles from the user.
    /// </summary>
    public List<int> RoleIds { get; set; } = new();
}