using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Employee.Entities;

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

    public async Task<IEnumerable<EmployeeEntity>> GetAllAsync()
    {
        const string sql = "SELECT * FROM Employee WHERE IsActive = 1 ORDER BY FullName";
        return await _context.QueryAsync<EmployeeEntity>(sql);
    }

    public async Task<IEnumerable<EmployeeEntity>> GetByDepartmentAsync(string department)
    {
        const string sql = "SELECT * FROM Employee WHERE Department = @Department AND IsActive = 1 ORDER BY FullName";
        return await _context.QueryAsync<EmployeeEntity>(sql, new { Department = department });
    }

    public async Task<IEnumerable<EmployeeEntity>> GetByStatusAsync(string employmentStatus)
    {
        const string sql = "SELECT * FROM Employee WHERE EmploymentStatus = @EmploymentStatus AND IsActive = 1 ORDER BY FullName";
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

    public async Task<string> GenerateEmployeeCodeAsync()
    {
        const string sql = @"
            SELECT ISNULL(MAX(CAST(SUBSTRING(EmployeeCode, 5, LEN(EmployeeCode)) AS INT)), 0) + 1 
            FROM Employee WHERE EmployeeCode LIKE 'EMP-%'";
        var nextNumber = await _context.ExecuteScalarAsync<int>(sql);
        return $"EMP-{nextNumber:D6}";
    }

    public async Task<int> GetActiveAssetsCountAsync(int employeeId)
    {
        const string sql = "SELECT COUNT(*) FROM Asset WHERE CurrentHolderId = @EmployeeId AND Status IN ('Assigned', 'On Loan') AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId });
    }

    // NEW
    public async Task<int> GetAssetsOnLoanCountAsync(int employeeId)
    {
        const string sql = @"
            SELECT COUNT(*) FROM Asset a
            INNER JOIN AssetTransaction t ON a.AssetId = t.AssetId
            WHERE a.CurrentHolderId = @EmployeeId 
              AND a.Status = 'On Loan' 
              AND a.IsActive = 1
              AND t.TransactionType = 'LOAN'
              AND t.TransactionStatus = 'Approved'
              AND t.PairedTransactionId IS NULL";

        return await _context.ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId });
    }

    // NEW
    public async Task<int> GetOverdueLoansCountAsync(int employeeId)
    {
        const string sql = @"
            SELECT COUNT(*) FROM AssetTransaction t
            INNER JOIN Asset a ON t.AssetId = a.AssetId
            WHERE t.ToEmployeeId = @EmployeeId
              AND t.TransactionType = 'LOAN'
              AND t.TransactionStatus = 'Approved'
              AND t.PairedTransactionId IS NULL
              AND t.ExpectedReturnDate < GETDATE()
              AND a.IsActive = 1";

        return await _context.ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId });
    }

    // NEW
    public async Task<int> GetTotalHistoricalAssetsAsync(int employeeId)
    {
        const string sql = @"
            SELECT COUNT(DISTINCT AssetId) FROM AssetTransaction
            WHERE FromEmployeeId = @EmployeeId OR ToEmployeeId = @EmployeeId";

        return await _context.ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId });
    }

    // NEW
    public async Task<int> GetReturnedAssetsCountAsync(int employeeId)
    {
        const string sql = @"
            SELECT COUNT(*) FROM AssetTransaction
            WHERE FromEmployeeId = @EmployeeId
              AND TransactionType IN ('RETURN', 'LOAN_RETURN')
              AND TransactionStatus = 'Approved'";

        return await _context.ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId });
    }

    // NEW
    public async Task<int> GetDamagedReturnsCountAsync(int employeeId)
    {
        const string sql = @"
            SELECT COUNT(*) FROM AssetTransaction
            WHERE FromEmployeeId = @EmployeeId
              AND TransactionType IN ('RETURN', 'LOAN_RETURN')
              AND DamageReason IS NOT NULL";

        return await _context.ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId });
    }
}