using Whitebird.Domain.Features.Employee.Entities;

namespace Whitebird.Infra.Features.Employee;

public interface IEmployeeReps
{
    Task<EmployeeEntity?> GetByIdAsync(int employeeId);
    Task<IEnumerable<EmployeeEntity>> GetAllAsync();
    Task<IEnumerable<EmployeeEntity>> GetByDepartmentAsync(string department);
    Task<IEnumerable<EmployeeEntity>> GetByStatusAsync(string employmentStatus);
    Task<bool> IsEmployeeCodeExistsAsync(string employeeCode, int? excludeEmployeeId = null);
    Task<string> GenerateEmployeeCodeAsync();
    Task<int> GetActiveAssetsCountAsync(int employeeId);

    // NEW
    Task<int> GetAssetsOnLoanCountAsync(int employeeId);
    Task<int> GetOverdueLoansCountAsync(int employeeId);
    Task<int> GetTotalHistoricalAssetsAsync(int employeeId);
    Task<int> GetReturnedAssetsCountAsync(int employeeId);
    Task<int> GetDamagedReturnsCountAsync(int employeeId);
}