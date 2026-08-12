using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Employee;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Employee;

/// <summary>
/// Repository implementation for Employee operations using Dapper
/// </summary>
public class EmployeeReps : IEmployeeReps
{
    private readonly DapperContext _context;

    public EmployeeReps(DapperContext context)
    {
        _context = context;
    }

    // ============================================================
    // CRUD - return Entity
    // ============================================================

    public async Task<EmployeeEntity?> GetByIdRawAsync(int employeeId)
    {
        const string sql = "SELECT * FROM Employee WHERE EmployeeId = @EmployeeId";
        return await _context.QueryFirstOrDefaultAsync<EmployeeEntity>(sql, new { EmployeeId = employeeId });
    }

    public async Task<bool> IsEmployeeCodeExistsAsync(string employeeCode, int? excludeEmployeeId = null)
    {
        var sql = "SELECT COUNT(1) FROM Employee WHERE EmployeeCode = @EmployeeCode";
        var parameters = new DynamicParameters();
        parameters.Add("@EmployeeCode", employeeCode);

        if (excludeEmployeeId.HasValue)
        {
            sql += " AND EmployeeId != @ExcludeEmployeeId";
            parameters.Add("@ExcludeEmployeeId", excludeEmployeeId.Value);
        }

        return await _context.ExecuteScalarAsync<int>(sql, parameters) > 0;
    }

    // ============================================================
    // GET METHODS - return View
    // ============================================================

    public async Task<EmployeeDetailView?> GetDetailByIdAsync(int employeeId)
    {
        const string sql = @"
            SELECT 
                e.EmployeeId, e.EmployeeCode, e.FullName, e.Address,
                e.DepartmentId, d.DepartmentName,
                e.Position, md1.MasterDataName as PositionName,
                e.EmploymentStatus, md2.MasterDataName as EmploymentStatusName,
                e.PhoneNumber, e.Email,
                e.OfficeId, o.OfficeName,
                e.JoinDate, e.ResignDate,
                e.IsActive, e.CreatedDate, e.CreatedBy, e.ModifiedDate, e.ModifiedBy
            FROM Employee e
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Office o ON e.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON e.Position = md1.ReferenceCode AND md1.ReferenceName = 'Position' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON e.EmploymentStatus = md2.ReferenceCode AND md2.ReferenceName = 'EmployeeStatus' AND md2.IsActive = 1
            WHERE e.EmployeeId = @EmployeeId";

        return await _context.QueryFirstOrDefaultAsync<EmployeeDetailView>(sql, new { EmployeeId = employeeId });
    }

    public async Task<IEnumerable<EmployeeListView>> GetAllListViewAsync()
    {
        const string sql = @"
            SELECT 
                e.EmployeeId, e.EmployeeCode, e.FullName, e.Address,
                e.DepartmentId, d.DepartmentName,
                e.Position, md1.MasterDataName as PositionName,
                e.EmploymentStatus, md2.MasterDataName as EmploymentStatusName,
                e.PhoneNumber, e.Email,
                e.OfficeId, o.OfficeName,
                e.JoinDate, e.ResignDate,
                e.IsActive, e.CreatedDate, e.CreatedBy, e.ModifiedDate, e.ModifiedBy
            FROM Employee e
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Office o ON e.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON e.Position = md1.ReferenceCode AND md1.ReferenceName = 'Position' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON e.EmploymentStatus = md2.ReferenceCode AND md2.ReferenceName = 'EmployeeStatus' AND md2.IsActive = 1
            ORDER BY e.FullName";

        return await _context.QueryAsync<EmployeeListView>(sql);
    }

    // ============================================================
    // PAGINATION - FIXED: tambah CanEdit dan CanDelete
    // ============================================================

    public async Task<PaginatedResult<EmployeeListView>> GetPagedListAsync(EmployeeGridRequest request)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "e.IsActive", request.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchColumns = new[] { "e.EmployeeCode", "e.FullName", "e.Email", "e.PhoneNumber" };
            GridQueryBuilder.AddSearchCondition(conditions, parameters, request.Search, searchColumns);
        }

        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "e.DepartmentId", request.DepartmentId);
        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "e.OfficeId", request.OfficeId);
        GridQueryBuilder.AddEqualsFilter(conditions, parameters, "e.EmploymentStatus", request.EmploymentStatus);

        var whereClause = GridQueryBuilder.BuildWhereClause(conditions, parameters);
        var orderBy = GridQueryBuilder.BuildOrderByClause(request.SortBy, request.SortDescending, "e.FullName", false);
        var pagination = GridQueryBuilder.BuildPaginationClause(request.Page, request.PageSize, parameters);

        var countSql = $@"
            SELECT COUNT(*) 
            FROM Employee e
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var selectClause = @"
            SELECT 
                e.EmployeeId, e.EmployeeCode, e.FullName, e.Address,
                e.DepartmentId, d.DepartmentName,
                e.Position, md1.MasterDataName as PositionName,
                e.EmploymentStatus, md2.MasterDataName as EmploymentStatusName,
                e.PhoneNumber, e.Email,
                e.OfficeId, o.OfficeName,
                e.JoinDate, e.ResignDate,
                e.IsActive, e.CreatedDate, e.CreatedBy, e.ModifiedDate, e.ModifiedBy,
                1 as CanEdit,
                1 as CanDelete";

        var fromClause = @"
            FROM Employee e
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Office o ON e.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON e.Position = md1.ReferenceCode AND md1.ReferenceName = 'Position' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON e.EmploymentStatus = md2.ReferenceCode AND md2.ReferenceName = 'EmployeeStatus' AND md2.IsActive = 1";

        var dataSql = $@"
            {selectClause}
            {fromClause}
            {whereClause}
            {orderBy}
            {pagination}";

        var data = await _context.QueryAsync<EmployeeListView>(dataSql, parameters);

        return new PaginatedResult<EmployeeListView>
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

    public async Task<IEnumerable<EmployeeDropdownView>> GetDropdownListAsync()
    {
        const string sql = @"
            SELECT EmployeeId, EmployeeCode, FullName
            FROM Employee
            WHERE IsActive = 1
            ORDER BY FullName";

        return await _context.QueryAsync<EmployeeDropdownView>(sql);
    }
}