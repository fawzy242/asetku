using Whitebird.Domain.Features.Department;

namespace Whitebird.Infra.Features.Department;

public interface IDepartmentReps
{
    Task<DepartmentEntity?> GetByIdAsync(int departmentId);
    Task<IEnumerable<DepartmentEntity>> GetAllAsync();
    Task<IEnumerable<DepartmentEntity>> GetActiveOnlyAsync();
    Task<bool> IsDepartmentNameExistsAsync(string departmentName, int? excludeDepartmentId = null);
    Task<bool> IsDepartmentCodeExistsAsync(string departmentCode, int? excludeDepartmentId = null);
    Task<int> GetEmployeeCountAsync(int departmentId);
}