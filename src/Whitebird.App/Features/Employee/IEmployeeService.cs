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
    Task<ServiceResult<EmployeeDetailView>> UpdateAsync(int id, EmployeeUpdateFormDto model);
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
    // NEW: PHOTO & ATTACHMENTS (for EmployeeSummary and standalone use)
    // ============================================================

    /// <summary>
    /// Gets profile photo bytes for an employee
    /// </summary>
    Task<ServiceResult<byte[]>> GetPhotoAsync(int employeeId);

    /// <summary>
    /// Gets attachment list for an employee
    /// </summary>
    Task<ServiceResult<IEnumerable<EmployeeAttachmentViewDto>>> GetAttachmentsAsync(int employeeId);
}