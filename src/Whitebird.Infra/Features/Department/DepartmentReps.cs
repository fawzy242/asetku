using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Department;
using Whitebird.Domain.Features.Common;
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
        const string sql = "SELECT * FROM Department WHERE DepartmentId = @DepartmentId";
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

    // ============================================================
    // PAGINATION - FIXED: tambah CanEdit dan CanDelete
    // ============================================================

    public async Task<PaginatedResult<DepartmentListView>> GetPagedListAsync(DepartmentGridRequest request)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "d.IsActive", request.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchColumns = new[] { "d.DepartmentName", "d.DepartmentCode", "d.Description" };
            GridQueryBuilder.AddSearchCondition(conditions, parameters, request.Search, searchColumns);
        }

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "d.DepartmentName", false);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        var countSql = $@"
            SELECT COUNT(*) 
            FROM Department d
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @"
            SELECT 
                d.DepartmentId, d.DepartmentCode, d.DepartmentName, d.Description,
                d.IsActive, d.CreatedDate, d.CreatedBy, d.ModifiedDate, d.ModifiedBy,
                (SELECT COUNT(*) FROM Employee WHERE DepartmentId = d.DepartmentId AND IsActive = 1) as EmployeeCount,
                1 as CanEdit,
                1 as CanDelete";

        var dataSql = $@"
            {selectClause}
            FROM Department d
            {whereClause}
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<DepartmentListView>(dataSql, parameters);

        return new PaginatedResult<DepartmentListView>
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