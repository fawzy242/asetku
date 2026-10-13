using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Department;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Department;

/// <summary>
/// Service interface for Department business logic
/// </summary>
public interface IDepartmentService
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    Task<ServiceResult<DepartmentDetailView>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<DepartmentListView>>> GetAllAsync();
    Task<ServiceResult<DepartmentDetailView>> CreateAsync(DepartmentCreateViewModel model);
    Task<ServiceResult<DepartmentDetailView>> UpdateAsync(int id, DepartmentUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID
    // ============================================================

    Task<ServiceResult<PaginatedResult<DepartmentListView>>> GetGridDataAsync(DepartmentGridRequest request);

    // ============================================================
    // DROPDOWN
    // ============================================================

    Task<ServiceResult<IEnumerable<DepartmentDropdownView>>> GetDropdownListAsync();

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeleteAsync(BulkDeleteRequest request);
}