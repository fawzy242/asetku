using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Office;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Office;

/// <summary>
/// Service interface for Office business logic
/// </summary>
public interface IOfficeService
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    Task<ServiceResult<OfficeDetailView>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<OfficeListView>>> GetAllAsync();
    Task<ServiceResult<OfficeDetailView>> CreateAsync(OfficeCreateViewModel model);
    Task<ServiceResult<OfficeDetailView>> UpdateAsync(int id, OfficeUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID
    // ============================================================

    Task<ServiceResult<PaginatedResult<OfficeListView>>> GetGridDataAsync(OfficeGridRequest request);

    // ============================================================
    // DROPDOWN
    // ============================================================

    Task<ServiceResult<IEnumerable<OfficeDropdownView>>> GetDropdownListAsync();

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request);
}