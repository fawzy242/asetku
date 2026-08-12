using Microsoft.AspNetCore.Http;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Department;
using Whitebird.Domain.Features.Employee;
using Whitebird.Domain.Features.MasterData;
using Whitebird.Domain.Features.Office;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Employee;

/// <summary>
/// Service interface for Employee business logic
/// </summary>
public interface IEmployeeService
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    Task<ServiceResult<EmployeeDetailView>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<EmployeeListView>>> GetAllAsync();
    Task<ServiceResult<EmployeeDetailView>> CreateAsync(EmployeeCreateViewModel model);
    Task<ServiceResult<EmployeeDetailView>> UpdateAsync(int id, EmployeeUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID
    // ============================================================

    Task<ServiceResult<PaginatedResult<EmployeeListView>>> GetGridDataAsync(EmployeeGridRequest request);

    // ============================================================
    // DROPDOWN
    // ============================================================

    Task<ServiceResult<IEnumerable<EmployeeDropdownView>>> GetDropdownListAsync();
    Task<ServiceResult<IEnumerable<OfficeDropdownView>>> GetOfficeListAsync();
    Task<ServiceResult<IEnumerable<DepartmentDropdownView>>> GetDepartmentListAsync();
    Task<ServiceResult<IEnumerable<MasterDataDto>>> GetMasterDataListAsync(string referenceName);

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request);

    // ============================================================
    // PHOTO PROFILE
    // ============================================================

    Task<ServiceResult> AddPhotoProfileAsync(int employeeId, IFormFile file);
    Task<ServiceResult> DeletePhotoProfileAsync(int employeeId);
    Task<ServiceResult<byte[]>> GetPhotoProfileAsync(int employeeId);
}