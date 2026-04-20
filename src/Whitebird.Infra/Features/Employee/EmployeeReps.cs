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
        const string sql = "SELECT ISNULL(MAX(CAST(SUBSTRING(EmployeeCode, 4, LEN(EmployeeCode)) AS INT)), 0) + 1 FROM Employee WHERE EmployeeCode LIKE 'EMP%'";
        var nextNumber = await _context.ExecuteScalarAsync<int>(sql);
        return $"EMP{nextNumber:D4}";
    }

    public async Task<int> GetActiveAssetsCountAsync(int employeeId)
    {
        const string sql = "SELECT COUNT(*) FROM Asset WHERE CurrentHolderId = @EmployeeId AND Status = 'Assigned' AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { EmployeeId = employeeId });
    }
}