using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Office;
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
        const string sql = "SELECT * FROM Office WHERE OfficeId = @OfficeId AND IsActive = 1";
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

    public async Task<IEnumerable<OfficeListView>> GetActiveOnlyListViewAsync()
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

    public async Task<IEnumerable<OfficeListView>> GetSubOfficesListViewAsync(int parentOfficeId)
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
            WHERE o.ParentOfficeId = @ParentOfficeId AND o.IsActive = 1
            ORDER BY o.OfficeName";
        
        return await _context.QueryAsync<OfficeListView>(sql, new { ParentOfficeId = parentOfficeId });
    }

    // ============================================================
    // GRID/LIST METHODS
    // ============================================================

    public async Task<PaginatedResult<OfficeListView>> GetPagedListAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        bool? isActiveFilter = null;
        if (filters != null && filters.ContainsKey("isActive"))
        {
            if (filters["isActive"] is bool isActive)
            {
                isActiveFilter = isActive;
            }
            filters.Remove("isActive");
        }

        if (isActiveFilter.HasValue)
        {
            conditions.Add($"o.IsActive = {(isActiveFilter.Value ? 1 : 0)}");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(@"(o.OfficeName LIKE @Search OR o.OfficeCode LIKE @Search OR o.City LIKE @Search OR o.Address LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        if (filters != null)
        {
            foreach (var filter in filters.Where(f => f.Value != null && !string.IsNullOrEmpty(f.Value.ToString())))
            {
                conditions.Add($"o.{filter.Key} = @{filter.Key}");
                parameters.Add($"@{filter.Key}", filter.Value);
            }
        }

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "";
        
        if (string.IsNullOrEmpty(sortBy))
        {
            sortBy = "o.OfficeName";
            sortDescending = false;
        }
        else
        {
            if (!sortBy.StartsWith("o.") && !sortBy.StartsWith("p."))
            {
                sortBy = $"o.{sortBy}";
            }
        }
        
        var orderBy = $"{sortBy} {(sortDescending ? "DESC" : "ASC")}";

        var countSql = $@"
            SELECT COUNT(*) 
            FROM Office o
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var offset = (page - 1) * pageSize;
        var dataSql = $@"
            SELECT 
                o.OfficeId,
                o.OfficeCode,
                o.OfficeName,
                o.OfficeType,
                md.MasterDataName as OfficeTypeName,
                o.City,
                o.Address,
                o.Phone,
                o.ParentOfficeId,
                p.OfficeName as ParentOfficeName,
                (SELECT COUNT(*) FROM Office WHERE ParentOfficeId = o.OfficeId AND IsActive = 1) as ChildCount,
                o.IsActive,
                o.CreatedDate,
                o.CreatedBy,
                o.ModifiedDate,
                o.ModifiedBy
            FROM Office o
            LEFT JOIN Office p ON o.ParentOfficeId = p.OfficeId
            LEFT JOIN MasterData md ON o.OfficeType = md.ReferenceCode AND md.ReferenceName = 'OfficeType' AND md.IsActive = 1
            {whereClause}
            ORDER BY {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var data = await _context.QueryAsync<OfficeListView>(dataSql, parameters);

        return new PaginatedResult<OfficeListView>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Filters = filters,
            SortBy = sortBy,
            SortDescending = sortDescending
        };
    }

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