using Whitebird.Domain.Features.Supplier;
using Whitebird.App.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Supplier;

/// <summary>
/// Service interface for Supplier business logic
/// </summary>
public interface ISupplierService
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    Task<ServiceResult<SupplierDetailView>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<SupplierListView>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<SupplierListView>>> GetActiveOnlyAsync();
    Task<ServiceResult<SupplierDetailView>> CreateAsync(SupplierCreateViewModel model);
    Task<ServiceResult<SupplierDetailView>> UpdateAsync(int id, SupplierUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID & DROPDOWN
    // ============================================================

    Task<ServiceResult<PaginatedResult<SupplierListView>>> GetGridDataAsync(
        int page, int pageSize, string? search = null, bool? isActive = null, Dictionary<string, object>? filters = null);
    
    Task<ServiceResult<IEnumerable<SupplierDropdownView>>> GetDropdownListAsync();
}