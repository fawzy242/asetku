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

    /// <summary>
    /// Gets a category by ID with all related data
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category detail view or not found result</returns>
    Task<ServiceResult<CategoryDetailView>> GetByIdAsync(int id);
    
    /// <summary>
    /// Gets all categories as list view
    /// </summary>
    /// <returns>Collection of category list views</returns>
    Task<ServiceResult<IEnumerable<CategoryListView>>> GetAllAsync();
    
    /// <summary>
    /// Gets active only categories as list view
    /// </summary>
    /// <returns>Collection of active category list views</returns>
    Task<ServiceResult<IEnumerable<CategoryListView>>> GetActiveOnlyAsync();
    
    /// <summary>
    /// Gets sub-categories by parent ID
    /// </summary>
    /// <param name="parentId">Parent category ID</param>
    /// <returns>Collection of sub-category list views</returns>
    Task<ServiceResult<IEnumerable<CategoryListView>>> GetSubCategoriesAsync(int parentId);
    
    /// <summary>
    /// Creates a new category
    /// </summary>
    /// <param name="model">Category creation data</param>
    /// <returns>Created category detail view</returns>
    Task<ServiceResult<CategoryDetailView>> CreateAsync(CategoryCreateViewModel model);
    
    /// <summary>
    /// Updates an existing category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <param name="model">Category update data</param>
    /// <returns>Updated category detail view</returns>
    Task<ServiceResult<CategoryDetailView>> UpdateAsync(int id, CategoryUpdateViewModel model);
    
    /// <summary>
    /// Permanently deletes a category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> DeleteAsync(int id);
    
    /// <summary>
    /// Soft deletes a category (sets IsActive = false)
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID & DROPDOWN
    // ============================================================

    /// <summary>
    /// Gets paginated list of categories for grid display
    /// </summary>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search keyword</param>
    /// <returns>Paginated result with category list views</returns>
    Task<ServiceResult<PaginatedResult<CategoryListView>>> GetGridDataAsync(int page, int pageSize, string? search = null);
    
    /// <summary>
    /// Gets category dropdown list (minimal data for selects)
    /// </summary>
    /// <returns>Collection of category dropdown views</returns>
    Task<ServiceResult<IEnumerable<CategoryDropdownView>>> GetDropdownListAsync();
}