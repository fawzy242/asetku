using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.UserRoles;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.UserRoles;

/// <summary>
/// Service interface for UserRoles business logic (role assignment to users).
/// </summary>
public interface IUserRolesService
{
    /// <summary>
    /// Gets the flat paginated grid of (user, role) assignments.
    /// </summary>
    Task<ServiceResult<PaginatedResult<UserRolesListView>>> GetGridDataAsync(UserRolesGridRequest request);

    /// <summary>
    /// Gets all active role assignments for the given user.
    /// </summary>
    Task<ServiceResult<IEnumerable<UserRolesListView>>> GetRolesByUserIdAsync(int userId);

    /// <summary>
    /// Gets all active user assignments for the given role.
    /// </summary>
    Task<ServiceResult<IEnumerable<UserRolesListView>>> GetUsersByRoleIdAsync(int roleId);

    /// <summary>
    /// Assigns a single role to a user. Fails with Conflict if the role is already
    /// actively assigned; reactivates a soft-deleted assignment if it exists.
    /// </summary>
    Task<ServiceResult<UserRolesListView>> AssignAsync(UserRolesAssignRequest request);

    /// <summary>
    /// Removes a single role from a user. Soft-delete.
    /// </summary>
    Task<ServiceResult> UnassignAsync(int userId, int roleId);

    /// <summary>
    /// Replaces the entire role set for a user with the supplied role ids.
    /// Empty list removes all roles from the user.
    /// </summary>
    Task<ServiceResult<IEnumerable<UserRolesListView>>> ReplaceForUserAsync(int userId, UserRolesReplaceRequest request);
}