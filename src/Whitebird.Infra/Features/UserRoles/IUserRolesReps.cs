using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.UserRoles;

namespace Whitebird.Infra.Features.UserRoles;

public interface IUserRolesReps
{
    // ============================================================
    // READS
    // ============================================================

    Task<IEnumerable<UserRolesListView>> GetAssignmentsByUserIdAsync(int userId);
    Task<IEnumerable<UserRolesListView>> GetAssignmentsByRoleIdAsync(int roleId);
    Task<PaginatedResult<UserRolesListView>> GetPagedListAsync(UserRolesGridRequest request);

    // ============================================================
    // VALIDATION HELPERS
    // ============================================================
    //
    // NOTE (P4-5): GetActiveUserCountForRoleAsync was removed — it had no callers
    // and duplicated IRolesReps.GetUserCountAsync. Use IRolesReps for role-level
    // user counts.

    Task<bool> HasActiveAssignmentAsync(int userId, int roleId);

    // ============================================================
    // WRITES
    // ============================================================

    Task<int> InsertAsync(UserRolesEntity entity);
    Task<int> ReactivateAsync(int userId, int roleId, string modifiedBy);
    Task<int> UnassignAsync(int userId, int roleId, string modifiedBy);
    Task<int> UnassignAllForUserAsync(int userId, string modifiedBy);
}