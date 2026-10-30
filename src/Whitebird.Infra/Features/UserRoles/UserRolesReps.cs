using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.UserRoles;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.UserRoles;

/// <summary>
/// Repository implementation for UserRoles operations using Dapper.
/// UserRoles is a join table between Users and Roles. Active assignments only
/// (IsActive = 1) are considered in reads.
/// </summary>
public class UserRolesReps : IUserRolesReps
{
    private readonly DapperContext _context;

    public UserRolesReps(DapperContext context)
    {
        _context = context;
    }

    // ============================================================
    // READS
    // ============================================================

    public async Task<IEnumerable<UserRolesListView>> GetAssignmentsByUserIdAsync(int userId)
    {
        const string sql = @"
            SELECT 
                ur.UserRoleId, ur.UserId, ur.RoleId,
                u.Username, u.FullName AS UserFullName, u.Email AS UserEmail,
                r.RoleCode, r.RoleName,
                ur.IsActive, ur.CreatedDate, ur.CreatedBy, ur.ModifiedDate, ur.ModifiedBy,
                1 AS CanDelete
            FROM UserRoles ur
            INNER JOIN Users u ON ur.UserId = u.UserId
            INNER JOIN Roles r ON ur.RoleId = r.RoleId
            WHERE ur.UserId = @UserId AND ur.IsActive = 1
            ORDER BY r.RoleName";

        return await _context.QueryAsync<UserRolesListView>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<UserRolesListView>> GetAssignmentsByRoleIdAsync(int roleId)
    {
        const string sql = @"
            SELECT 
                ur.UserRoleId, ur.UserId, ur.RoleId,
                u.Username, u.FullName AS UserFullName, u.Email AS UserEmail,
                r.RoleCode, r.RoleName,
                ur.IsActive, ur.CreatedDate, ur.CreatedBy, ur.ModifiedDate, ur.ModifiedBy,
                1 AS CanDelete
            FROM UserRoles ur
            INNER JOIN Users u ON ur.UserId = u.UserId
            INNER JOIN Roles r ON ur.RoleId = r.RoleId
            WHERE ur.RoleId = @RoleId AND ur.IsActive = 1
            ORDER BY u.FullName";

        return await _context.QueryAsync<UserRolesListView>(sql, new { RoleId = roleId });
    }

    public async Task<PaginatedResult<UserRolesListView>> GetPagedListAsync(UserRolesGridRequest request)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "ur.IsActive", request.IsActive);
        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "ur.UserId", request.UserId);
        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "ur.RoleId", request.RoleId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchColumns = new[]
            {
                "u.Username", "u.FullName", "u.Email", "r.RoleCode", "r.RoleName"
            };
            GridQueryBuilder.AddSearchCondition(conditions, parameters, request.Search, searchColumns);
        }

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "ur.CreatedDate", true);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        var countSql = $@"
            SELECT COUNT(*)
            FROM UserRoles ur
            INNER JOIN Users u ON ur.UserId = u.UserId
            INNER JOIN Roles r ON ur.RoleId = r.RoleId
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @"
            SELECT 
                ur.UserRoleId, ur.UserId, ur.RoleId,
                u.Username, u.FullName AS UserFullName, u.Email AS UserEmail,
                r.RoleCode, r.RoleName,
                ur.IsActive, ur.CreatedDate, ur.CreatedBy, ur.ModifiedDate, ur.ModifiedBy,
                1 AS CanDelete";

        var dataSql = $@"
            {selectClause}
            FROM UserRoles ur
            INNER JOIN Users u ON ur.UserId = u.UserId
            INNER JOIN Roles r ON ur.RoleId = r.RoleId
            {whereClause}
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<UserRolesListView>(dataSql, parameters);

        return new PaginatedResult<UserRolesListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    // ============================================================
    // VALIDATION HELPERS
    // ============================================================

    public async Task<bool> HasActiveAssignmentAsync(int userId, int roleId)
    {
        const string sql = @"
            SELECT COUNT(1) FROM UserRoles 
            WHERE UserId = @UserId AND RoleId = @RoleId AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { UserId = userId, RoleId = roleId }) > 0;
    }

    // NOTE (P4-5): GetActiveUserCountForRoleAsync was removed — it had no callers
    // and duplicated IRolesReps.GetUserCountAsync.

    // ============================================================
    // WRITES
    // ============================================================

    public async Task<int> InsertAsync(UserRolesEntity entity)
    {
        const string sql = @"
            INSERT INTO UserRoles (
                UserId, RoleId, IsActive, CreatedDate, CreatedBy
            ) VALUES (
                @UserId, @RoleId, 1, GETDATE(), @CreatedBy
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await _context.ExecuteScalarAsync<int>(sql, entity);
    }

    public async Task<int> ReactivateAsync(int userId, int roleId, string modifiedBy)
    {
        // Reuses an existing soft-deleted assignment row so the UNIQUE (UserId, RoleId)
        // constraint is never violated when a role is re-assigned after being removed.
        const string sql = @"
            UPDATE UserRoles 
            SET IsActive = 1, ModifiedDate = GETDATE(), ModifiedBy = @ModifiedBy
            WHERE UserId = @UserId AND RoleId = @RoleId AND IsActive = 0";

        return await _context.ExecuteAsync(sql, new { UserId = userId, RoleId = roleId, ModifiedBy = modifiedBy });
    }

    public async Task<int> UnassignAsync(int userId, int roleId, string modifiedBy)
    {
        const string sql = @"
            UPDATE UserRoles 
            SET IsActive = 0, ModifiedDate = GETDATE(), ModifiedBy = @ModifiedBy
            WHERE UserId = @UserId AND RoleId = @RoleId AND IsActive = 1";

        return await _context.ExecuteAsync(sql, new { UserId = userId, RoleId = roleId, ModifiedBy = modifiedBy });
    }

    public async Task<int> UnassignAllForUserAsync(int userId, string modifiedBy)
    {
        const string sql = @"
            UPDATE UserRoles 
            SET IsActive = 0, ModifiedDate = GETDATE(), ModifiedBy = @ModifiedBy
            WHERE UserId = @UserId AND IsActive = 1";

        return await _context.ExecuteAsync(sql, new { UserId = userId, ModifiedBy = modifiedBy });
    }
}