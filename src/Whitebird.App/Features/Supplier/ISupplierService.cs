using Whitebird.Domain.Features.Supplier;
using Whitebird.Domain.Features.Common;
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
    Task<ServiceResult<SupplierDetailView>> CreateAsync(SupplierCreateViewModel model);
    Task<ServiceResult<SupplierDetailView>> UpdateAsync(int id, SupplierUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID
    // ============================================================

    Task<ServiceResult<PaginatedResult<SupplierListView>>> GetGridDataAsync(SupplierGridRequest request);

    // ============================================================
    // DROPDOWN
    // ============================================================

    Task<ServiceResult<IEnumerable<SupplierDropdownView>>> GetDropdownListAsync();

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeleteAsync(BulkDeleteRequest request);
}