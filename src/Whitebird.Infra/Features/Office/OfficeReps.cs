using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Office;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Office;

/// <summary>
/// Repository implementation for Office operations using Dapper
/// </summary>
public class OfficeReps : IOfficeReps
{
    private readonly DapperContext _context;

    public OfficeReps(DapperContext context)
    {
        _context = context;
    }

    // ============================================================
    // CRUD - return Entity
    // ============================================================

    public async Task<OfficeEntity?> GetByIdRawAsync(int officeId)
    {
        const string sql = "SELECT * FROM Office WHERE OfficeId = @OfficeId";
        return await _context.QueryFirstOrDefaultAsync<OfficeEntity>(sql, new { OfficeId = officeId });
    }

    public async Task<bool> IsOfficeCodeExistsAsync(string officeCode, int? excludeOfficeId = null)
    {
        if (string.IsNullOrWhiteSpace(officeCode))
            return false;

        var sql = "SELECT COUNT(1) FROM Office WHERE OfficeCode = @OfficeCode AND IsActive = 1";
        var parameters = new DynamicParameters();
        parameters.Add("@OfficeCode", officeCode);

        if (excludeOfficeId.HasValue)
        {
            sql += " AND OfficeId != @ExcludeOfficeId";
            parameters.Add("@ExcludeOfficeId", excludeOfficeId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<bool> IsOfficeNameExistsAsync(string officeName, int? excludeOfficeId = null)
    {
        var sql = "SELECT COUNT(1) FROM Office WHERE OfficeName = @OfficeName AND IsActive = 1";
        var parameters = new DynamicParameters();
        parameters.Add("@OfficeName", officeName);

        if (excludeOfficeId.HasValue)
        {
            sql += " AND OfficeId != @ExcludeOfficeId";
            parameters.Add("@ExcludeOfficeId", excludeOfficeId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<int> GetChildCountAsync(int officeId)
    {
        const string sql = "SELECT COUNT(*) FROM Office WHERE ParentOfficeId = @OfficeId AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { OfficeId = officeId });
    }

    // ============================================================
    // GET METHODS - return View
    // ============================================================

    public async Task<OfficeDetailView?> GetDetailByIdAsync(int officeId)
    {
        const string sql = @"
            SELECT 
                o.OfficeId, o.OfficeCode, o.OfficeName, o.OfficeType,
                md.MasterDataName as OfficeTypeName,
                o.City, o.Address, o.Phone,
                o.ParentOfficeId, p.OfficeName as ParentOfficeName,
                (SELECT COUNT(*) FROM Office WHERE ParentOfficeId = o.OfficeId AND IsActive = 1) as ChildCount,
                o.IsActive, o.CreatedDate, o.CreatedBy, o.ModifiedDate, o.ModifiedBy
            FROM Office o
            LEFT JOIN Office p ON o.ParentOfficeId = p.OfficeId
            LEFT JOIN MasterData md ON o.OfficeType = md.ReferenceCode AND md.ReferenceName = 'OfficeType' AND md.IsActive = 1
            WHERE o.OfficeId = @OfficeId AND o.IsActive = 1";

        return await _context.QueryFirstOrDefaultAsync<OfficeDetailView>(sql, new { OfficeId = officeId });
    }

    public async Task<IEnumerable<OfficeListView>> GetAllListViewAsync()
    {
        const string sql = @"
            SELECT 
                o.OfficeId, o.OfficeCode, o.OfficeName, o.OfficeType,
                md.MasterDataName as OfficeTypeName,
                o.City, o.Address, o.Phone,
                o.ParentOfficeId, p.OfficeName as ParentOfficeName,
                (SELECT COUNT(*) FROM Office WHERE ParentOfficeId = o.OfficeId AND IsActive = 1) as ChildCount,
                o.IsActive, o.CreatedDate, o.CreatedBy, o.ModifiedDate, o.ModifiedBy
            FROM Office o
            LEFT JOIN Office p ON o.ParentOfficeId = p.OfficeId
            LEFT JOIN MasterData md ON o.OfficeType = md.ReferenceCode AND md.ReferenceName = 'OfficeType' AND md.IsActive = 1
            WHERE o.IsActive = 1
            ORDER BY o.OfficeName";

        return await _context.QueryAsync<OfficeListView>(sql);
    }

    // ============================================================
    // PAGINATION - FIXED: tambah CanEdit dan CanDelete
    // ============================================================

    public async Task<PaginatedResult<OfficeListView>> GetPagedListAsync(OfficeGridRequest request)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "o.IsActive", request.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchColumns = new[] { "o.OfficeName", "o.OfficeCode", "o.City", "o.Address", "o.Phone" };
            GridQueryBuilder.AddSearchCondition(conditions, parameters, request.Search, searchColumns);
        }

        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "o.OfficeType", request.OfficeType);

        if (request.ParentOfficeId.HasValue)
        {
            if (request.ParentOfficeId.Value == 0)
            {
                conditions.Add("o.ParentOfficeId IS NULL");
            }
            else
            {
                conditions.Add("o.ParentOfficeId = @ParentOfficeId");
                parameters.Add("@ParentOfficeId", request.ParentOfficeId.Value);
            }
        }

        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "o.City", request.City);

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "o.OfficeName", false);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        var countSql = $@"
            SELECT COUNT(*) 
            FROM Office o
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @"
            SELECT 
                o.OfficeId, o.OfficeCode, o.OfficeName, o.OfficeType,
                md.MasterDataName as OfficeTypeName,
                o.City, o.Address, o.Phone,
                o.ParentOfficeId, p.OfficeName as ParentOfficeName,
                (SELECT COUNT(*) FROM Office WHERE ParentOfficeId = o.OfficeId AND IsActive = 1) as ChildCount,
                o.IsActive, o.CreatedDate, o.CreatedBy, o.ModifiedDate, o.ModifiedBy,
                1 as CanEdit,
                1 as CanDelete";

        var fromClause = @"
            FROM Office o
            LEFT JOIN Office p ON o.ParentOfficeId = p.OfficeId
            LEFT JOIN MasterData md ON o.OfficeType = md.ReferenceCode AND md.ReferenceName = 'OfficeType' AND md.IsActive = 1";

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<OfficeListView>(dataSql, parameters);

        return new PaginatedResult<OfficeListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    // ============================================================
    // DROPDOWN
    // ============================================================

    public async Task<IEnumerable<OfficeDropdownView>> GetDropdownListAsync()
    {
        const string sql = @"
            SELECT OfficeId, OfficeName, OfficeCode, ParentOfficeId
            FROM Office
            WHERE IsActive = 1
            ORDER BY OfficeName";

        return await _context.QueryAsync<OfficeDropdownView>(sql);
    }
}