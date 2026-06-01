using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Employee;
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

    public async Task<IEnumerable<EmployeeListView>> GetActiveOnlyListViewAsync()
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
            WHERE e.IsActive = 1
            ORDER BY e.FullName";
        
        return await _context.QueryAsync<EmployeeListView>(sql);
    }

    public async Task<IEnumerable<EmployeeListView>> GetByDepartmentIdListViewAsync(int departmentId)
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
            WHERE e.DepartmentId = @DepartmentId
            ORDER BY e.FullName";
        
        return await _context.QueryAsync<EmployeeListView>(sql, new { DepartmentId = departmentId });
    }

    public async Task<IEnumerable<EmployeeListView>> GetByEmploymentStatusListViewAsync(int employmentStatus)
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
            WHERE e.EmploymentStatus = @EmploymentStatus
            ORDER BY e.FullName";
        
        return await _context.QueryAsync<EmployeeListView>(sql, new { EmploymentStatus = employmentStatus });
    }

    // ============================================================
    // ASSET STATISTICS
    // ============================================================

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

    // ============================================================
    // ASSET SUMMARY
    // ============================================================

    public async Task<EmployeeAssetSummaryView?> GetAssetSummaryByEmployeeIdAsync(int employeeId)
    {
        const string employeeSql = @"
            SELECT e.EmployeeId, e.EmployeeCode, e.FullName,
                   d.DepartmentName,
                   md2.MasterDataName as EmploymentStatusName
            FROM Employee e
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN MasterData md2 ON e.EmploymentStatus = md2.ReferenceCode AND md2.ReferenceName = 'EmployeeStatus' AND md2.IsActive = 1
            WHERE e.EmployeeId = @EmployeeId AND e.IsActive = 1";

        var employee = await _context.QueryFirstOrDefaultAsync<dynamic>(employeeSql, new { EmployeeId = employeeId });
        
        if (employee == null)
        {
            return null;
        }

        var result = new EmployeeAssetSummaryView
        {
            EmployeeId = employee.EmployeeId,
            EmployeeCode = employee.EmployeeCode,
            FullName = employee.FullName,
            DepartmentName = employee.DepartmentName,
            EmploymentStatusName = employee.EmploymentStatusName,
            CurrentlyHeldAssets = await GetActiveAssetsCountAsync(employeeId),
            AssetsOnLoan = await GetAssetsOnLoanCountAsync(employeeId),
            OverdueLoans = await GetOverdueLoansCountAsync(employeeId),
            TotalHistoricalAssets = await GetTotalHistoricalAssetsAsync(employeeId),
            ReturnedAssets = await GetReturnedAssetsCountAsync(employeeId),
            DamagedReturns = await GetDamagedReturnsCountAsync(employeeId),
            CurrentAssets = new List<EmployeeCurrentAssetView>(),
            AssetHistory = new List<EmployeeAssetHistoryView>()
        };

        const string currentAssetsSql = @"
            SELECT DISTINCT 
                a.AssetId, a.AssetCode, a.AssetName,
                c.CategoryName,
                CASE 
                    WHEN at.TransactionType = 3 THEN 'On Loan'
                    ELSE 'Assigned'
                END as Status,
                CASE 
                    WHEN at.TransactionType = 3 THEN 'On Loan'
                    ELSE 'Assigned'
                END as AssociationType,
                at.TransactionDate as SinceDate,
                at.ExpectedReturnDate,
                CASE 
                    WHEN at.ExpectedReturnDate < GETDATE() AND at.TransactionType = 3 THEN 1
                    ELSE 0
                END as IsOverdue,
                md.MasterDataName as ConditionName,
                a.PurchasePrice
            FROM Asset a
            INNER JOIN AssetTransaction at ON a.AssetId = at.AssetId
            LEFT JOIN Category c ON a.CategoryId = c.CategoryId AND c.IsActive = 1
            LEFT JOIN MasterData md ON a.AssetCondition = md.ReferenceCode AND md.ReferenceName = 'AssetCondition' AND md.IsActive = 1
            WHERE at.ToEmployeeId = @EmployeeId
              AND at.Approved = 1
              AND at.FromAssetTransactionId IS NULL
              AND at.TransactionType IN (1, 2, 3)
              AND a.IsActive = 1
            ORDER BY at.TransactionDate DESC";

        var currentAssets = await _context.QueryAsync<EmployeeCurrentAssetView>(currentAssetsSql, new { EmployeeId = employeeId });
        result.CurrentAssets = currentAssets.ToList();

        const string historySql = @"
            SELECT 
                at.AssetTransactionId,
                at.AssetId,
                a.AssetCode,
                a.AssetName,
                md1.MasterDataName as TransactionTypeName,
                at.TransactionDate,
                fe.FullName as FromEmployeeName,
                te.FullName as ToEmployeeName,
                md2.MasterDataName as ConditionAfterName,
                at.Notes
            FROM AssetTransaction at
            INNER JOIN Asset a ON at.AssetId = a.AssetId
            LEFT JOIN Employee fe ON at.FromEmployeeId = fe.EmployeeId
            LEFT JOIN Employee te ON at.ToEmployeeId = te.EmployeeId
            LEFT JOIN MasterData md1 ON at.TransactionType = md1.ReferenceCode AND md1.ReferenceName = 'TransactionType' AND md1.IsActive = 1
            LEFT JOIN MasterData md2 ON at.ConditionAfter = md2.ReferenceCode AND md2.ReferenceName = 'AssetCondition' AND md2.IsActive = 1
            WHERE (at.FromEmployeeId = @EmployeeId OR at.ToEmployeeId = @EmployeeId)
              AND at.IsActive = 1
            ORDER BY at.TransactionDate DESC";

        var history = await _context.QueryAsync<EmployeeAssetHistoryView>(historySql, new { EmployeeId = employeeId });
        result.AssetHistory = history.ToList();

        return result;
    }

    // ============================================================
    // GRID/LIST METHODS
    // ============================================================

    public async Task<PaginatedResult<EmployeeListView>> GetPagedListAsync(
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
            conditions.Add($"e.IsActive = {(isActiveFilter.Value ? 1 : 0)}");
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add(@"(e.EmployeeCode LIKE @Search OR e.FullName LIKE @Search OR e.Email LIKE @Search OR e.PhoneNumber LIKE @Search)");
            parameters.Add("@Search", $"%{search}%");
        }

        if (filters != null)
        {
            foreach (var filter in filters.Where(f => f.Value != null && !string.IsNullOrEmpty(f.Value.ToString())))
            {
                conditions.Add($"e.{filter.Key} = @{filter.Key}");
                parameters.Add($"@{filter.Key}", filter.Value);
            }
        }

        var whereClause = conditions.Any() ? $"WHERE {string.Join(" AND ", conditions)}" : "";
        
        if (string.IsNullOrEmpty(sortBy))
        {
            sortBy = "e.FullName";
            sortDescending = false;
        }
        else
        {
            if (!sortBy.StartsWith("e.") && !sortBy.StartsWith("d.") && !sortBy.StartsWith("o."))
            {
                sortBy = $"e.{sortBy}";
            }
        }
        
        var orderBy = $"{sortBy} {(sortDescending ? "DESC" : "ASC")}";

        var countSql = $@"
            SELECT COUNT(*) 
            FROM Employee e
            {whereClause}";
        var totalCount = await _context.ExecuteScalarAsync<int>(countSql, parameters);

        var offset = (page - 1) * pageSize;
        var dataSql = $@"
            SELECT 
                e.EmployeeId,
                e.EmployeeCode,
                e.FullName,
                e.Address,
                e.DepartmentId,
                d.DepartmentName,
                e.Position,
                md1.MasterDataName as PositionName,
                e.EmploymentStatus,
                md2.MasterDataName as EmploymentStatusName,
                e.PhoneNumber,
                e.Email,
                e.OfficeId,
                o.OfficeName,
                e.JoinDate,
                e.ResignDate,
                e.IsActive,
                e.CreatedDate,
                e.CreatedBy,
                e.ModifiedDate,
                e.ModifiedBy
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

        var data = await _context.QueryAsync<EmployeeListView>(dataSql, parameters);

        return new PaginatedResult<EmployeeListView>
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