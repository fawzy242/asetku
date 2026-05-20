using Whitebird.Domain.Features.Employee;

namespace Whitebird.Infra.Features.Employee;

public interface IEmployeeReps
{
    Task<EmployeeEntity?> GetByIdAsync(int employeeId);
    Task<EmployeeEntity?> GetByIdWithRelationsAsync(int employeeId);
    Task<IEnumerable<EmployeeEntity>> GetAllAsync();
    Task<IEnumerable<EmployeeEntity>> GetByDepartmentIdAsync(int departmentId);
    Task<IEnumerable<EmployeeEntity>> GetByEmploymentStatusAsync(int employmentStatus);
    Task<bool> IsEmployeeCodeExistsAsync(string employeeCode, int? excludeEmployeeId = null);
    Task<int> GetActiveAssetsCountAsync(int employeeId);
    Task<int> GetAssetsOnLoanCountAsync(int employeeId);
    Task<int> GetOverdueLoansCountAsync(int employeeId);
    Task<int> GetTotalHistoricalAssetsAsync(int employeeId);
    Task<int> GetReturnedAssetsCountAsync(int employeeId);
    Task<int> GetDamagedReturnsCountAsync(int employeeId);
}