using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Department;
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
    Task<ServiceResult<IEnumerable<DepartmentListView>>> GetActiveOnlyAsync();
    Task<ServiceResult<DepartmentDetailView>> CreateAsync(DepartmentCreateViewModel model);
    Task<ServiceResult<DepartmentDetailView>> UpdateAsync(int id, DepartmentUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID & DROPDOWN
    // ============================================================

    Task<ServiceResult<PaginatedResult<DepartmentListView>>> GetGridDataAsync(
        int page, int pageSize, string? search = null, bool? isActive = null, Dictionary<string, object>? filters = null);
    
    Task<ServiceResult<IEnumerable<DepartmentDropdownView>>> GetDropdownListAsync();
}