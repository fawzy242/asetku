using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Department;

namespace Whitebird.Infra.Features.Department;

public class DepartmentReps : IDepartmentReps
{
    private readonly DapperContext _context;

    public DepartmentReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<DepartmentEntity?> GetByIdAsync(int departmentId)
    {
        const string sql = "SELECT * FROM Department WHERE DepartmentId = @DepartmentId AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<DepartmentEntity>(sql, new { DepartmentId = departmentId });
    }

    public async Task<IEnumerable<DepartmentEntity>> GetAllAsync()
    {
        const string sql = "SELECT * FROM Department WHERE IsActive = 1 ORDER BY DepartmentName";
        return await _context.QueryAsync<DepartmentEntity>(sql);
    }

    public async Task<IEnumerable<DepartmentEntity>> GetActiveOnlyAsync()
    {
        const string sql = "SELECT * FROM Department WHERE IsActive = 1 ORDER BY DepartmentName";
        return await _context.QueryAsync<DepartmentEntity>(sql);
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
}