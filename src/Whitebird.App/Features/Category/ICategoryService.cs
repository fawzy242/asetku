using Whitebird.Domain.Features.Category;
using Whitebird.App.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Category;

/// <summary>
/// Service interface for Category business logic
/// </summary>
public interface ICategoryService
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    Task<ServiceResult<CategoryDetailView>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<CategoryListView>>> GetAllAsync();
    Task<ServiceResult<CategoryDetailView>> CreateAsync(CategoryCreateViewModel model);
    Task<ServiceResult<CategoryDetailView>> UpdateAsync(int id, CategoryUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID
    // ============================================================

    Task<ServiceResult<PaginatedResult<CategoryListView>>> GetGridDataAsync(CategoryGridRequest request);

    // ============================================================
    // DROPDOWN
    // ============================================================

    Task<ServiceResult<IEnumerable<CategoryDropdownView>>> GetDropdownListAsync();

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request);
}