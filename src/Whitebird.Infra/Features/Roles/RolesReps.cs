using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.Roles;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Roles;

/// <summary>
/// Repository implementation for Roles operations using Dapper
/// </summary>
public class RolesReps : IRolesReps
{
    private readonly DapperContext _context;

    public RolesReps(DapperContext context)
    {
        _context = context;
    }

    // ============================================================
    // RAW ENTITY
    // ============================================================

    public async Task<RolesEntity?> GetByIdRawAsync(int roleId)
    {
        const string sql = "SELECT * FROM Roles WHERE RoleId = @RoleId";
        return await _context.QueryFirstOrDefaultAsync<RolesEntity>(sql, new { RoleId = roleId });
    }

    public async Task<bool> IsRoleCodeExistsAsync(string roleCode, int? excludeRoleId = null)
    {
        if (string.IsNullOrWhiteSpace(roleCode))
            return false;

        var sql = "SELECT COUNT(1) FROM Roles WHERE RoleCode = @RoleCode";
        var parameters = new DynamicParameters();
        parameters.Add("@RoleCode", roleCode);

        if (excludeRoleId.HasValue)
        {
            sql += " AND RoleId != @ExcludeRoleId";
            parameters.Add("@ExcludeRoleId", excludeRoleId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<bool> IsRoleNameExistsAsync(string roleName, int? excludeRoleId = null)
    {
        var sql = "SELECT COUNT(1) FROM Roles WHERE RoleName = @RoleName";
        var parameters = new DynamicParameters();
        parameters.Add("@RoleName", roleName);

        if (excludeRoleId.HasValue)
        {
            sql += " AND RoleId != @ExcludeRoleId";
            parameters.Add("@ExcludeRoleId", excludeRoleId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<int> GetUserCountAsync(int roleId)
    {
        const string sql = "SELECT COUNT(*) FROM UserRoles WHERE RoleId = @RoleId AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { RoleId = roleId });
    }

    // ============================================================
    // VIEWS
    // ============================================================

    public async Task<RolesDetailView?> GetDetailByIdAsync(int roleId)
    {
        // No IsActive filter: detail-by-id must return the record in any state.
        const string sql = @"
            SELECT 
                r.RoleId, r.RoleCode, r.RoleName, r.Description,
                (SELECT COUNT(*) FROM UserRoles ur WHERE ur.RoleId = r.RoleId AND ur.IsActive = 1) AS AssignedUserCount,
                r.IsActive, r.CreatedDate, r.CreatedBy, r.ModifiedDate, r.ModifiedBy
            FROM Roles r
            WHERE r.RoleId = @RoleId";

        return await _context.QueryFirstOrDefaultAsync<RolesDetailView>(sql, new { RoleId = roleId });
    }

    public async Task<IEnumerable<RolesListView>> GetAllListViewAsync()
    {
        const string sql = @"
            SELECT 
                r.RoleId, r.RoleCode, r.RoleName, r.Description,
                (SELECT COUNT(*) FROM UserRoles ur WHERE ur.RoleId = r.RoleId AND ur.IsActive = 1) AS AssignedUserCount,
                r.IsActive, r.CreatedDate, r.CreatedBy, r.ModifiedDate, r.ModifiedBy,
                1 AS CanEdit,
                1 AS CanDelete
            FROM Roles r
            WHERE r.IsActive = 1
            ORDER BY r.RoleName";

        return await _context.QueryAsync<RolesListView>(sql);
    }

    public async Task<PaginatedResult<RolesListView>> GetPagedListAsync(RolesGridRequest request)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "r.IsActive", request.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchColumns = new[] { "r.RoleCode", "r.RoleName", "r.Description" };
            GridQueryBuilder.AddSearchCondition(conditions, parameters, request.Search, searchColumns);
        }

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "r.RoleName", false);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        var countSql = $@"
            SELECT COUNT(*)
            FROM Roles r
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @"
            SELECT 
                r.RoleId, r.RoleCode, r.RoleName, r.Description,
                (SELECT COUNT(*) FROM UserRoles ur WHERE ur.RoleId = r.RoleId AND ur.IsActive = 1) AS AssignedUserCount,
                r.IsActive, r.CreatedDate, r.CreatedBy, r.ModifiedDate, r.ModifiedBy,
                1 AS CanEdit,
                1 AS CanDelete";

        var dataSql = $@"
            {selectClause}
            FROM Roles r
            {whereClause}
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<RolesListView>(dataSql, parameters);

        return new PaginatedResult<RolesListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    public async Task<IEnumerable<RolesDropdownView>> GetDropdownListAsync()
    {
        const string sql = @"
            SELECT RoleId, RoleCode, RoleName
            FROM Roles
            WHERE IsActive = 1
            ORDER BY RoleName";

        return await _context.QueryAsync<RolesDropdownView>(sql);
    }
}