using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.Roles;

namespace Whitebird.Infra.Features.Roles;

public interface IRolesReps
{
    // ============================================================
    // RAW ENTITY
    // ============================================================

    Task<RolesEntity?> GetByIdRawAsync(int roleId);
    Task<bool> IsRoleCodeExistsAsync(string roleCode, int? excludeRoleId = null);
    Task<bool> IsRoleNameExistsAsync(string roleName, int? excludeRoleId = null);
    Task<int> GetUserCountAsync(int roleId);

    // ============================================================
    // VIEWS
    // ============================================================

    Task<RolesDetailView?> GetDetailByIdAsync(int roleId);
    Task<IEnumerable<RolesListView>> GetAllListViewAsync();
    Task<PaginatedResult<RolesListView>> GetPagedListAsync(RolesGridRequest request);
    Task<IEnumerable<RolesDropdownView>> GetDropdownListAsync();
}