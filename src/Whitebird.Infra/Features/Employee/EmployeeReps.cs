using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Employee;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Employee;

public class EmployeeReps : IEmployeeReps
{
    private readonly DapperContext _context;

    public EmployeeReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<EmployeeEntity?> GetByIdAsync(int employeeId)
    {
        const string sql = "SELECT * FROM Employee WHERE EmployeeId = @EmployeeId";
        return await _context.QueryFirstOrDefaultAsync<EmployeeEntity>(sql, new { EmployeeId = employeeId });
    }

    public async Task<EmployeeEntity?> GetByIdWithRelationsAsync(int employeeId)
    {
        const string sql = @"
            SELECT e.*, 
                   d.DepartmentName, 
                   o.OfficeName,
                   md1.MasterDataName as PositionName,
                   md2.MasterDataName as EmploymentStatusName
            FROM Employee e
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Office o ON e.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON e.Position = md1.ReferenceCode AND md1.ReferenceName = 'Position' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON e.EmploymentStatus = md2.ReferenceCode AND md2.ReferenceName = 'EmployeeStatus' AND md2.IsActive = 1
            WHERE e.EmployeeId = @EmployeeId";
        return await _context.QueryFirstOrDefaultAsync<EmployeeEntity>(sql, new { EmployeeId = employeeId });
    }

    public async Task<IEnumerable<EmployeeEntity>> GetAllAsync()
    {
        const string sql = @"
            SELECT e.*, 
                   d.DepartmentName, 
                   o.OfficeName,
                   md1.MasterDataName as PositionName,
                   md2.MasterDataName as EmploymentStatusName
            FROM Employee e
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Office o ON e.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON e.Position = md1.ReferenceCode AND md1.ReferenceName = 'Position' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON e.EmploymentStatus = md2.ReferenceCode AND md2.ReferenceName = 'EmployeeStatus' AND md2.IsActive = 1
            ORDER BY e.FullName";
        return await _context.QueryAsync<EmployeeEntity>(sql);
    }

    public async Task<IEnumerable<EmployeeEntity>> GetActiveOnlyAsync()
    {
        const string sql = @"
            SELECT e.*, 
                   d.DepartmentName, 
                   o.OfficeName,
                   md1.MasterDataName as PositionName,
                   md2.MasterDataName as EmploymentStatusName
            FROM Employee e
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Office o ON e.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON e.Position = md1.ReferenceCode AND md1.ReferenceName = 'Position' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON e.EmploymentStatus = md2.ReferenceCode AND md2.ReferenceName = 'EmployeeStatus' AND md2.IsActive = 1
            WHERE e.IsActive = 1
            ORDER BY e.FullName";
        return await _context.QueryAsync<EmployeeEntity>(sql);
    }

    public async Task<IEnumerable<EmployeeEntity>> GetByDepartmentIdAsync(int departmentId)
    {
        const string sql = @"
            SELECT e.*, d.DepartmentName, o.OfficeName,
                   md1.MasterDataName as PositionName,
                   md2.MasterDataName as EmploymentStatusName
            FROM Employee e
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Office o ON e.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON e.Position = md1.ReferenceCode AND md1.ReferenceName = 'Position' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON e.EmploymentStatus = md2.ReferenceCode AND md2.ReferenceName = 'EmployeeStatus' AND md2.IsActive = 1
            WHERE e.DepartmentId = @DepartmentId
            ORDER BY e.FullName";
        return await _context.QueryAsync<EmployeeEntity>(sql, new { DepartmentId = departmentId });
    }

    public async Task<IEnumerable<EmployeeEntity>> GetByEmploymentStatusAsync(int employmentStatus)
    {
        const string sql = @"
            SELECT e.*, d.DepartmentName, o.OfficeName,
                   md1.MasterDataName as PositionName,
                   md2.MasterDataName as EmploymentStatusName
            FROM Employee e
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Office o ON e.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON e.Position = md1.ReferenceCode AND md1.ReferenceName = 'Position' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON e.EmploymentStatus = md2.ReferenceCode AND md2.ReferenceName = 'EmployeeStatus' AND md2.IsActive = 1
            WHERE e.EmploymentStatus = @EmploymentStatus
            ORDER BY e.FullName";
        return await _context.QueryAsync<EmployeeEntity>(sql, new { EmploymentStatus = employmentStatus });
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

    public async Task<int> GetActiveAssetsCountAsync(int employeeId)
    {
        const string sql = @"
            SELECT COUNT(DISTINCT t.AssetId) 
            FROM AssetTransaction t
            WHERE t.ToEmployeeId = @EmployeeId
              AND t.Approved = 1
              AND t.FromAssetTransactionId IS NULL
              AND t.TransactionType IN (1, 2, 3)
              AND t.IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId });
    }

    public async Task<int> GetAssetsOnLoanCountAsync(int employeeId)
    {
        const string sql = @"
            SELECT COUNT(DISTINCT t.AssetId) 
            FROM AssetTransaction t
            WHERE t.ToEmployeeId = @EmployeeId 
              AND t.TransactionType = 3
              AND t.Approved = 1
              AND t.FromAssetTransactionId IS NULL
              AND t.IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId });
    }

    public async Task<int> GetOverdueLoansCountAsync(int employeeId)
    {
        const string sql = @"
            SELECT COUNT(*) 
            FROM AssetTransaction t
            WHERE t.ToEmployeeId = @EmployeeId
              AND t.TransactionType = 3
              AND t.Approved = 1
              AND t.FromAssetTransactionId IS NULL
              AND t.ExpectedReturnDate < GETDATE()
              AND t.IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId });
    }

    public async Task<int> GetTotalHistoricalAssetsAsync(int employeeId)
    {
        const string sql = @"
            SELECT COUNT(DISTINCT AssetId) 
            FROM AssetTransaction
            WHERE (FromEmployeeId = @EmployeeId OR ToEmployeeId = @EmployeeId)
              AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId });
    }

    public async Task<int> GetReturnedAssetsCountAsync(int employeeId)
    {
        const string sql = @"
            SELECT COUNT(*) 
            FROM AssetTransaction
            WHERE FromEmployeeId = @EmployeeId
              AND TransactionType IN (4, 5)
              AND Approved = 1
              AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId });
    }

    public async Task<int> GetDamagedReturnsCountAsync(int employeeId)
    {
        const string sql = @"
            SELECT COUNT(*) 
            FROM AssetTransaction
            WHERE FromEmployeeId = @EmployeeId
              AND TransactionType IN (4, 5)
              AND ConditionAfter = 3
              AND Approved = 1
              AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId });
    }

    public async Task<PaginatedResult<EmployeeEntity>> GetPagedWithRelationsAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null)
    {
        var conditions = new List<string>();
        var parameters = new DynamicParameters();

        // Handle isActive filter
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
            conditions.Add($"e.IsActive = {(isActiveFilter.Value ? 1 : 0)}");
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(@"(e.EmployeeCode LIKE @Search OR e.FullName LIKE @Search OR e.Email LIKE @Search OR e.PhoneNumber LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        // Apply other filters (departmentId, etc.)
        if (filters != null)
        {
            foreach (var filter in filters.Where(f => f.Value != null && !string.IsNullOrEmpty(f.Value.ToString())))
            {
                conditions.Add($"e.{filter.Key} = @{filter.Key}");
                parameters.Add($"@{filter.Key}", filter.Value);
            }
        }

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "";
        sortBy = string.IsNullOrEmpty(sortBy) ? "e.FullName" : $"e.{sortBy}";
        var orderBy = $"{sortBy} {(sortDescending ? "DESC" : "ASC")}";

        var countSql = $@"
            SELECT COUNT(*) 
            FROM Employee e
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var offset = (page - 1) * pageSize;
        var dataSql = $@"
            SELECT e.*, d.DepartmentName, o.OfficeName,
                   md1.MasterDataName as PositionName,
                   md2.MasterDataName as EmploymentStatusName
            FROM Employee e
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Office o ON e.OfficeId = o.OfficeId AND o.IsActive = 1
            LEFT JOIN MasterData md1 ON e.Position = md1.ReferenceCode AND md1.ReferenceName = 'Position' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON e.EmploymentStatus = md2.ReferenceCode AND md2.ReferenceName = 'EmployeeStatus' AND md2.IsActive = 1
            {whereClause}
            ORDER BY {orderBy}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        var data = await _context.QueryAsync<EmployeeEntity>(dataSql, parameters);

        return new PaginatedResult<EmployeeEntity>
        {
            Data = data.ToList(),
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}