using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Employee;

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
        const string sql = "SELECT * FROM Employee WHERE EmployeeId = @EmployeeId AND IsActive = 1";
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
            WHERE e.EmployeeId = @EmployeeId AND e.IsActive = 1";
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
            WHERE e.IsActive = 1
            ORDER BY e.FullName";
        return await _context.QueryAsync<EmployeeEntity>(sql);
    }

    public async Task<IEnumerable<EmployeeEntity>> GetByDepartmentIdAsync(int departmentId)
    {
        const string sql = @"
            SELECT e.*, d.DepartmentName, o.OfficeName
            FROM Employee e
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Office o ON e.OfficeId = o.OfficeId AND o.IsActive = 1
            WHERE e.DepartmentId = @DepartmentId AND e.IsActive = 1
            ORDER BY e.FullName";
        return await _context.QueryAsync<EmployeeEntity>(sql, new { DepartmentId = departmentId });
    }

    public async Task<IEnumerable<EmployeeEntity>> GetByEmploymentStatusAsync(int employmentStatus)
    {
        const string sql = @"
            SELECT e.*, d.DepartmentName, o.OfficeName
            FROM Employee e
            LEFT JOIN Department d ON e.DepartmentId = d.DepartmentId AND d.IsActive = 1
            LEFT JOIN Office o ON e.OfficeId = o.OfficeId AND o.IsActive = 1
            WHERE e.EmploymentStatus = @EmploymentStatus AND e.IsActive = 1
            ORDER BY e.FullName";
        return await _context.QueryAsync<EmployeeEntity>(sql, new { EmploymentStatus = employmentStatus });
    }

    public async Task<bool> IsEmployeeCodeExistsAsync(string employeeCode, int? excludeEmployeeId = null)
    {
        var sql = "SELECT COUNT(1) FROM Employee WHERE EmployeeCode = @EmployeeCode AND IsActive = 1";
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
}