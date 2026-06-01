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
    Task<ServiceResult<IEnumerable<CategoryListView>>> GetActiveOnlyAsync();
    Task<ServiceResult<IEnumerable<CategoryListView>>> GetSubCategoriesAsync(int parentId);
    Task<ServiceResult<CategoryDetailView>> CreateAsync(CategoryCreateViewModel model);
    Task<ServiceResult<CategoryDetailView>> UpdateAsync(int id, CategoryUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID & DROPDOWN
    // ============================================================

    Task<ServiceResult<PaginatedResult<CategoryListView>>> GetGridDataAsync(
        int page, int pageSize, string? search = null, bool? isActive = null, Dictionary<string, object>? filters = null);
    
    Task<ServiceResult<IEnumerable<CategoryDropdownView>>> GetDropdownListAsync();
}