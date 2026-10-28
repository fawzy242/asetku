using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.Roles;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Roles;

namespace Whitebird.App.Features.Roles;

public class RolesService
    : CommonService<RolesEntity, RolesDetailView, RolesListView, RolesGridRequest, RolesCreateViewModel, RolesUpdateViewModel>,
      IRolesService
{
    private readonly IRolesReps _rolesReps;

    public RolesService(
        ICommonReps repository,
        IRolesReps rolesReps,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<RolesService> logger)
        : base(repository, currentUserService, activityLogService, logger)
    {
        _rolesReps = rolesReps;
    }

    protected override string EntityLabel => "Role";
    protected override string TableName => TableNames.Roles;

    protected override string BulkDeactivateEmptyMessage =>
        "No roles were deactivated. Roles with assigned users cannot be deactivated.";
    protected override string BulkDeleteEmptyMessage =>
        "No roles were deleted. Roles with assigned users cannot be deleted.";

    protected override async Task<RolesDetailView?> LoadDetailAsync(int id) =>
        await _rolesReps.GetDetailByIdAsync(id);

    protected override async Task<IEnumerable<RolesListView>> LoadAllAsync() =>
        await _rolesReps.GetAllListViewAsync();

    protected override async Task<PaginatedResult<RolesListView>> LoadPageAsync(RolesGridRequest request) =>
        await _rolesReps.GetPagedListAsync(request);

    protected override async Task<ServiceResult<RolesDetailView>?> ValidateCreateAsync(RolesCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.RoleName))
            return ServiceResult<RolesDetailView>.BadRequest("Role name is required");

        if (await _rolesReps.IsRoleNameExistsAsync(model.RoleName))
            return ServiceResult<RolesDetailView>.Conflict($"Role '{model.RoleName}' already exists");

        if (!string.IsNullOrWhiteSpace(model.RoleCode) &&
            await _rolesReps.IsRoleCodeExistsAsync(model.RoleCode))
            return ServiceResult<RolesDetailView>.Conflict($"Role code '{model.RoleCode}' already exists");

        return null;
    }

    protected override async Task<ServiceResult<RolesDetailView>?> ValidateUpdateAsync(int id, RolesUpdateViewModel model)
    {
        if (await _rolesReps.IsRoleNameExistsAsync(model.RoleName, id))
            return ServiceResult<RolesDetailView>.Conflict($"Role '{model.RoleName}' already exists");

        if (!string.IsNullOrWhiteSpace(model.RoleCode) &&
            await _rolesReps.IsRoleCodeExistsAsync(model.RoleCode, id))
            return ServiceResult<RolesDetailView>.Conflict($"Role code '{model.RoleCode}' already exists");

        return null;
    }

    protected override void MapCreate(RolesCreateViewModel model, RolesEntity entity) => model.Adapt(entity);
    protected override void MapUpdate(RolesUpdateViewModel model, RolesEntity entity) => model.Adapt(entity);
    protected override string Describe(RolesEntity entity) => entity.RoleName;

    protected override async Task<(bool CanProceed, string? Reason)> CanDeleteAsync(int id)
    {
        var userCount = await _rolesReps.GetUserCountAsync(id);
        return userCount > 0
            ? (false, $"Cannot delete role with {userCount} assigned users")
            : (true, (string?)null);
    }

    protected override async Task<(bool CanProceed, string? Reason)> CanDeactivateAsync(int id)
    {
        var userCount = await _rolesReps.GetUserCountAsync(id);
        return userCount > 0 ? (false, (string?)null) : (true, (string?)null);
    }

    public async Task<ServiceResult<IEnumerable<RolesDropdownView>>> GetDropdownListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var roles = await _rolesReps.GetDropdownListAsync();
            return ServiceResult<IEnumerable<RolesDropdownView>>.Success(roles);
        }, "get role dropdown list");
    }
}