using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Office;
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
    Task<ServiceResult<IEnumerable<OfficeListView>>> GetActiveOnlyAsync();
    Task<ServiceResult<IEnumerable<OfficeListView>>> GetSubOfficesAsync(int parentId);
    Task<ServiceResult<OfficeDetailView>> CreateAsync(OfficeCreateViewModel model);
    Task<ServiceResult<OfficeDetailView>> UpdateAsync(int id, OfficeUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID & DROPDOWN
    // ============================================================

    Task<ServiceResult<PaginatedResult<OfficeListView>>> GetGridDataAsync(
        int page, int pageSize, string? search = null, bool? isActive = null, Dictionary<string, object>? filters = null);
    
    Task<ServiceResult<IEnumerable<OfficeDropdownView>>> GetDropdownListAsync();
}