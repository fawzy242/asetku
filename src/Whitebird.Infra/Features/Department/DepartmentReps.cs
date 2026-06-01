using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Department;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Department;

/// <summary>
/// Repository implementation for Department operations using Dapper
/// </summary>
public class DepartmentReps : IDepartmentReps
{
    private readonly DapperContext _context;

    public DepartmentReps(DapperContext context)
    {
        _context = context;
    }

    // ============================================================
    // CRUD - return Entity
    // ============================================================

    public async Task<DepartmentEntity?> GetByIdRawAsync(int departmentId)
    {
        const string sql = "SELECT * FROM Department WHERE DepartmentId = @DepartmentId AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<DepartmentEntity>(sql, new { DepartmentId = departmentId });
    }

    public async Task<bool> IsDepartmentNameExistsAsync(string departmentName, int? excludeDepartmentId = null)
    {
        var sql = "SELECT COUNT(1) FROM Department WHERE DepartmentName = @DepartmentName AND IsActive = 1";
        var parameters = new DynamicParameters();
        parameters.Add("@DepartmentName", departmentName);

        if (excludeDepartmentId.HasValue)
        {
            sql += " AND DepartmentId != @ExcludeDepartmentId";
            parameters.Add("@ExcludeDepartmentId", excludeDepartmentId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<bool> IsDepartmentCodeExistsAsync(string departmentCode, int? excludeDepartmentId = null)
    {
        if (string.IsNullOrWhiteSpace(departmentCode))
            return false;

        var sql = "SELECT COUNT(1) FROM Department WHERE DepartmentCode = @DepartmentCode AND IsActive = 1";
        var parameters = new DynamicParameters();
        parameters.Add("@DepartmentCode", departmentCode);

        if (excludeDepartmentId.HasValue)
        {
            sql += " AND DepartmentId != @ExcludeDepartmentId";
            parameters.Add("@ExcludeDepartmentId", excludeDepartmentId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    public async Task<int> GetEmployeeCountAsync(int departmentId)
    {
        const string sql = "SELECT COUNT(*) FROM Employee WHERE DepartmentId = @DepartmentId AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { DepartmentId = departmentId });
    }

    // ============================================================
    // GET METHODS - return View
    // ============================================================

    public async Task<DepartmentDetailView?> GetDetailByIdAsync(int departmentId)
    {
        const string sql = @"
            SELECT 
                d.DepartmentId, d.DepartmentCode, d.DepartmentName, d.Description,
                d.IsActive, d.CreatedDate, d.CreatedBy, d.ModifiedDate, d.ModifiedBy,
                (SELECT COUNT(*) FROM Employee WHERE DepartmentId = d.DepartmentId AND IsActive = 1) as EmployeeCount
            FROM Department d
            WHERE d.DepartmentId = @DepartmentId AND d.IsActive = 1";
        
        return await _context.QueryFirstOrDefaultAsync<DepartmentDetailView>(sql, new { DepartmentId = departmentId });
    }

    public async Task<IEnumerable<DepartmentListView>> GetAllListViewAsync()
    {
        const string sql = @"
            SELECT 
                d.DepartmentId, d.DepartmentCode, d.DepartmentName, d.Description,
                d.IsActive, d.CreatedDate, d.CreatedBy, d.ModifiedDate, d.ModifiedBy,
                (SELECT COUNT(*) FROM Employee WHERE DepartmentId = d.DepartmentId AND IsActive = 1) as EmployeeCount
            FROM Department d
            WHERE d.IsActive = 1
            ORDER BY d.DepartmentName";
        
        return await _context.QueryAsync<DepartmentListView>(sql);
    }

    public async Task<IEnumerable<DepartmentListView>> GetActiveOnlyListViewAsync()
    {
        const string sql = @"
            SELECT 
                d.DepartmentId, d.DepartmentCode, d.DepartmentName, d.Description,
                d.IsActive, d.CreatedDate, d.CreatedBy, d.ModifiedDate, d.ModifiedBy,
                (SELECT COUNT(*) FROM Employee WHERE DepartmentId = d.DepartmentId AND IsActive = 1) as EmployeeCount
            FROM Department d
            WHERE d.IsActive = 1
            ORDER BY d.DepartmentName";
        
        return await _context.QueryAsync<DepartmentListView>(sql);
    }

    // ============================================================
    // GRID/LIST METHODS
    // ============================================================

    private string BuildBaseDepartmentQueryWithPagination(string selectClause, string whereClause, string orderBy, int offset, int pageSize)
    {
        return $@"
            {selectClause}
            FROM Department d
            {whereClause}
            {orderBy}
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";
    }

    private string BuildDepartmentListViewSelectClause()
    {
        return @"
            SELECT 
                d.DepartmentId,
                d.DepartmentCode,
                d.DepartmentName,
                d.Description,
                d.IsActive,
                d.CreatedDate,
                d.CreatedBy,
                d.ModifiedDate,
                d.ModifiedBy,
                (SELECT COUNT(*) FROM Employee WHERE DepartmentId = d.DepartmentId AND IsActive = 1) as EmployeeCount";
    }

    private (string WhereClause, DynamicParameters Parameters) BuildDepartmentWhereClause(
        string? search = null,
        bool? isActive = null,
        Dictionary<string, object>? additionalFilters = null)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        // Handle isActive filter
        if (isActive.HasValue)
        {
            conditions.Add($"d.IsActive = {(isActive.Value ? 1 : 0)}");
        }
        // No default - show all departments if not specified

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(@"(d.DepartmentName LIKE @Search OR d.DepartmentCode LIKE @Search OR d.Description LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        if (additionalFilters != null)
        {
            foreach (var filter in additionalFilters.Where(f => f.Value != null && !string.IsNullOrEmpty(f.Value.ToString())))
            {
                conditions.Add($"d.{filter.Key} = @{filter.Key}");
                parameters.Add($"@{filter.Key}", filter.Value);
            }
        }

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "";
        return (whereClause, parameters);
    }

    public async Task<PaginatedResult<DepartmentListView>> GetPagedListAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null)
    {
        bool? isActiveFilter = null;

        if (filters != null)
        {
            if (filters.ContainsKey("isActive") && bool.TryParse(filters["isActive"]?.ToString(), out bool isActive))
            {
                isActiveFilter = isActive;
            }
            filters?.Remove("isActive");
        }

        var (whereClause, parameters) = BuildDepartmentWhereClause(search, isActiveFilter, filters);

        if (string.IsNullOrEmpty(sortBy))
        {
            sortBy = "d.DepartmentName";
            sortDescending = false;
        }
        else
        {
            if (!sortBy.StartsWith("d."))
            {
                sortBy = $"d.{sortBy}";
            }
        }
        
        var orderBy = $"ORDER BY {sortBy} {(sortDescending ? "DESC" : "ASC")}";

        var countSql = $@"
            SELECT COUNT(*) 
            FROM Department d
            {whereClause}";
        
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = BuildDepartmentListViewSelectClause();
        var offset = (page - 1) * pageSize;
        var dataSql = BuildBaseDepartmentQueryWithPagination(selectClause, whereClause, orderBy, offset, pageSize);

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var data = await _context.QueryAsync<DepartmentListView>(dataSql, parameters);

        return new PaginatedResult<DepartmentListView>
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

    public async Task<IEnumerable<DepartmentDropdownView>> GetDropdownListAsync()
    {
        const string sql = @"
            SELECT DepartmentId, DepartmentName, DepartmentCode
            FROM Department
            WHERE IsActive = 1
            ORDER BY DepartmentName";

        return await _context.QueryAsync<DepartmentDropdownView>(sql);
    }
}