using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Common;

/// <summary>
/// Standard CRUD service contract for conventional menu modules.
///
/// Applied only to menus whose create / update / delete / grid / bulk flows are
/// a direct projection of the shared pattern (Category, Department, Office,
/// Supplier, Roles). Menus with attachments, workflow transitions, sessions,
/// generation, exports, or aggregates retain their own service interface.
/// </summary>
public interface ICommonService<TDetailView, TListView, TGridRequest, TCreateVM, TUpdateVM>
    where TDetailView : class
    where TListView : class
    where TGridRequest : GridRequestBase
    where TCreateVM : class
    where TUpdateVM : class
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    Task<ServiceResult<TDetailView>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<TListView>>> GetAllAsync();
    Task<ServiceResult<TDetailView>> CreateAsync(TCreateVM model);
    Task<ServiceResult<TDetailView>> UpdateAsync(int id, TUpdateVM model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID
    // ============================================================

    Task<ServiceResult<PaginatedResult<TListView>>> GetGridDataAsync(TGridRequest request);

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeleteAsync(BulkDeleteRequest request);
}