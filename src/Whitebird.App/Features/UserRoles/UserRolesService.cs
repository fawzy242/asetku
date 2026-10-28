using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.UserRoles;
using Whitebird.Infra.Features.Roles;
using Whitebird.Infra.Features.User;
using Whitebird.Infra.Features.UserRoles;

namespace Whitebird.App.Features.UserRoles;

/// <summary>
/// Service implementation for UserRoles business logic.
///
/// UserRoles is a join table. Writes never physically delete rows: unassign performs
/// a soft delete (IsActive = 0), and re-assigning an existing soft-deleted pair
/// reactivates it. This keeps the UQ_UserRoles_UserId_RoleId constraint satisfied
/// while preserving an audit trail.
///
/// Hardening applied (Batch H1):
///   * AssignAsync catches SQL unique-constraint exceptions (2601/2627) and
///     converts them into a clean 409 Conflict. The HasActiveAssignmentAsync
///     advisory check plus the DB constraint can race on concurrent calls;
///     previously the loser of the race received a 500.
/// </summary>
public class UserRolesService : BaseService, IUserRolesService
{
    private readonly IUserRolesReps _userRolesReps;
    private readonly IRolesReps _rolesReps;
    private readonly IUserReps _userReps;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public UserRolesService(
        IUserRolesReps userRolesReps,
        IRolesReps rolesReps,
        IUserReps userReps,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<UserRolesService> logger) : base(logger)
    {
        _userRolesReps = userRolesReps;
        _rolesReps = rolesReps;
        _userReps = userReps;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    // ============================================================
    // READS
    // ============================================================

    public async Task<ServiceResult<PaginatedResult<UserRolesListView>>> GetGridDataAsync(UserRolesGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _userRolesReps.GetPagedListAsync(request);
            return ServiceResult<PaginatedResult<UserRolesListView>>.Success(result);
        }, "get user role grid data");
    }

    public async Task<ServiceResult<IEnumerable<UserRolesListView>>> GetRolesByUserIdAsync(int userId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _userReps.GetUserByIdAsync(userId);
            if (user == null)
            {
                return ServiceResult<IEnumerable<UserRolesListView>>.NotFound($"User with id {userId} not found");
            }

            var roles = await _userRolesReps.GetAssignmentsByUserIdAsync(userId);
            return ServiceResult<IEnumerable<UserRolesListView>>.Success(roles);
        }, "get roles by user");
    }

    public async Task<ServiceResult<IEnumerable<UserRolesListView>>> GetUsersByRoleIdAsync(int roleId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var role = await _rolesReps.GetByIdRawAsync(roleId);
            if (role == null)
            {
                return ServiceResult<IEnumerable<UserRolesListView>>.NotFound($"Role with id {roleId} not found");
            }

            var users = await _userRolesReps.GetAssignmentsByRoleIdAsync(roleId);
            return ServiceResult<IEnumerable<UserRolesListView>>.Success(users);
        }, "get users by role");
    }

    // ============================================================
    // ASSIGN
    // ============================================================

    public async Task<ServiceResult<UserRolesListView>> AssignAsync(UserRolesAssignRequest request)
    {
        if (request == null)
        {
            return ServiceResult<UserRolesListView>.BadRequest("Request is required");
        }

        if (request.UserId <= 0)
        {
            return ServiceResult<UserRolesListView>.BadRequest("UserId must be greater than 0");
        }

        if (request.RoleId <= 0)
        {
            return ServiceResult<UserRolesListView>.BadRequest("RoleId must be greater than 0");
        }

        var user = await _userReps.GetUserByIdAsync(request.UserId);
        if (user == null)
        {
            return ServiceResult<UserRolesListView>.BadRequest($"User with id {request.UserId} does not exist");
        }

        if (!user.IsActive)
        {
            return ServiceResult<UserRolesListView>.BadRequest($"User '{user.Username}' is inactive");
        }

        var role = await _rolesReps.GetByIdRawAsync(request.RoleId);
        if (role == null)
        {
            return ServiceResult<UserRolesListView>.BadRequest($"Role with id {request.RoleId} does not exist");
        }

        if (!role.IsActive)
        {
            return ServiceResult<UserRolesListView>.BadRequest($"Role '{role.RoleName}' is inactive and cannot be assigned");
        }

        var alreadyAssigned = await _userRolesReps.HasActiveAssignmentAsync(request.UserId, request.RoleId);
        if (alreadyAssigned)
        {
            return ServiceResult<UserRolesListView>.Conflict(
                $"Role '{role.RoleName}' is already assigned to user '{user.Username}'");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            // Prefer reactivation if a soft-deleted assignment exists; otherwise insert.
            // A concurrent call between the advisory check above and this write can
            // still lose the unique-constraint race — convert that to a clean 409.
            try
            {
                var reactivated = await _userRolesReps.ReactivateAsync(
                    request.UserId, request.RoleId, _currentUserService.GetDisplayName());

                if (reactivated <= 0)
                {
                    await _userRolesReps.InsertAsync(new UserRolesEntity
                    {
                        UserId = request.UserId,
                        RoleId = request.RoleId,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        CreatedBy = _currentUserService.GetDisplayName()
                    });
                }
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                return ServiceResult<UserRolesListView>.Conflict(
                    $"Role '{role.RoleName}' is already assigned to user '{user.Username}'");
            }

            await _activityLogService.LogCreateAsync(
                TableNames.Users,
                request.UserId,
                $"Role '{role.RoleName}' assigned to user '{user.Username}'",
                _currentUserService.GetDisplayName());

            // Read back the row so the caller gets a fully-joined view.
            var assignments = await _userRolesReps.GetAssignmentsByUserIdAsync(request.UserId);
            var created = assignments.FirstOrDefault(a => a.RoleId == request.RoleId);

            return created == null
                ? ServiceResult<UserRolesListView>.Failure("Assignment created but could not be retrieved")
                : ServiceResult<UserRolesListView>.Success(created, "Role assigned successfully");
        }, "assign role to user");
    }

    // ============================================================
    // UNASSIGN
    // ============================================================

    public async Task<ServiceResult> UnassignAsync(int userId, int roleId)
    {
        if (userId <= 0 || roleId <= 0)
        {
            return ServiceResult.BadRequest("UserId and RoleId must be greater than 0");
        }

        var user = await _userReps.GetUserByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult.NotFound($"User with id {userId} not found");
        }

        var hasActive = await _userRolesReps.HasActiveAssignmentAsync(userId, roleId);
        if (!hasActive)
        {
            return ServiceResult.NotFound($"Role {roleId} is not assigned to user {userId}");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            await _userRolesReps.UnassignAsync(userId, roleId, _currentUserService.GetDisplayName());

            var role = await _rolesReps.GetByIdRawAsync(roleId);
            var roleName = role?.RoleName ?? roleId.ToString();

            await _activityLogService.LogDeleteAsync(
                TableNames.Users,
                userId,
                $"Role '{roleName}' unassigned from user '{user.Username}'",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Role unassigned successfully");
        }, "unassign role from user");
    }

    // ============================================================
    // REPLACE
    // ============================================================

    public async Task<ServiceResult<IEnumerable<UserRolesListView>>> ReplaceForUserAsync(int userId, UserRolesReplaceRequest request)
    {
        if (userId <= 0)
        {
            return ServiceResult<IEnumerable<UserRolesListView>>.BadRequest("UserId must be greater than 0");
        }

        var user = await _userReps.GetUserByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult<IEnumerable<UserRolesListView>>.BadRequest($"User with id {userId} does not exist");
        }

        if (!user.IsActive)
        {
            return ServiceResult<IEnumerable<UserRolesListView>>.BadRequest($"User '{user.Username}' is inactive");
        }

        // Deduplicate and validate every requested role exists and is active.
        var requestedRoleIds = (request?.RoleIds ?? new List<int>()).Distinct().ToList();

        foreach (var roleId in requestedRoleIds)
        {
            if (roleId <= 0)
            {
                return ServiceResult<IEnumerable<UserRolesListView>>.BadRequest($"Invalid RoleId: {roleId}");
            }

            var role = await _rolesReps.GetByIdRawAsync(roleId);
            if (role == null)
            {
                return ServiceResult<IEnumerable<UserRolesListView>>.BadRequest($"Role with id {roleId} does not exist");
            }

            if (!role.IsActive)
            {
                return ServiceResult<IEnumerable<UserRolesListView>>.BadRequest(
                    $"Role '{role.RoleName}' is inactive and cannot be assigned");
            }
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = (await _userRolesReps.GetAssignmentsByUserIdAsync(userId)).ToList();
            var existingRoleIds = existing.Select(e => e.RoleId).ToList();

            var toRemove = existing.Where(e => !requestedRoleIds.Contains(e.RoleId)).ToList();
            var toAdd = requestedRoleIds.Where(r => !existingRoleIds.Contains(r)).ToList();

            foreach (var assignment in toRemove)
            {
                await _userRolesReps.UnassignAsync(userId, assignment.RoleId, _currentUserService.GetDisplayName());
                await _activityLogService.LogDeleteAsync(
                    TableNames.Users,
                    userId,
                    $"Role '{assignment.RoleName}' unassigned from user '{user.Username}'",
                    _currentUserService.GetDisplayName());
            }

            foreach (var roleId in toAdd)
            {
                try
                {
                    var reactivated = await _userRolesReps.ReactivateAsync(
                        userId, roleId, _currentUserService.GetDisplayName());

                    if (reactivated <= 0)
                    {
                        await _userRolesReps.InsertAsync(new UserRolesEntity
                        {
                            UserId = userId,
                            RoleId = roleId,
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                            CreatedBy = _currentUserService.GetDisplayName()
                        });
                    }
                }
                catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
                {
                    // Two concurrent replace calls hit the same (UserId, RoleId). This
                    // call loses the race; the other call has already assigned it. That
                    // is the correct end state for a replace semantics — keep going
                    // rather than surfacing a 500 for a benign race.
                    continue;
                }

                var role = await _rolesReps.GetByIdRawAsync(roleId);
                var roleName = role?.RoleName ?? roleId.ToString();
                await _activityLogService.LogCreateAsync(
                    TableNames.Users,
                    userId,
                    $"Role '{roleName}' assigned to user '{user.Username}'",
                    _currentUserService.GetDisplayName());
            }

            var finalAssignments = await _userRolesReps.GetAssignmentsByUserIdAsync(userId);
            var message = $"{toRemove.Count} role(s) removed, {toAdd.Count} role(s) added";

            return ServiceResult<IEnumerable<UserRolesListView>>.Success(finalAssignments, message);
        }, "replace user roles");
    }
}