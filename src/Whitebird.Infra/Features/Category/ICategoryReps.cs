using Whitebird.Domain.Features.Category;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Category;

/// <summary>
/// Repository interface for Category operations
/// </summary>
public interface ICategoryReps
{
    // ============================================================
    // RAW ENTITY METHODS (For internal Service use only - NOT for API)
    // ============================================================
    
    /// <summary>
    /// Gets category entity by ID (RAW - returns Entity, for internal use only)
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>Category entity or null if not found</returns>
    Task<CategoryEntity?> GetByIdRawAsync(int categoryId);
    
    /// <summary>
    /// Checks if category name already exists
    /// </summary>
    /// <param name="categoryName">Category name to check</param>
    /// <param name="excludeCategoryId">Optional category ID to exclude (for updates)</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> IsCategoryNameExistsAsync(string categoryName, int? excludeCategoryId = null);
    
    /// <summary>
    /// Checks if category code already exists
    /// </summary>
    /// <param name="categoryCode">Category code to check</param>
    /// <param name="excludeCategoryId">Optional category ID to exclude (for updates)</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> IsCategoryCodeExistsAsync(string categoryCode, int? excludeCategoryId = null);
    
    /// <summary>
    /// Gets child count for a category
    /// </summary>
    /// <param name="categoryId">Parent category ID</param>
    /// <returns>Number of child categories</returns>
    Task<int> GetChildCountAsync(int categoryId);
    
    // ============================================================
    // DETAIL VIEW METHODS (For API responses)
    // ============================================================
    
    /// <summary>
    /// Gets category detail view by ID (includes parent info and child count)
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>Category detail view or null if not found</returns>
    Task<CategoryDetailView?> GetDetailByIdAsync(int categoryId);
    
    /// <summary>
    /// Gets all categories as list view
    /// </summary>
    /// <returns>Collection of category list views</returns>
    Task<IEnumerable<CategoryListView>> GetAllListViewAsync();
    
    /// <summary>
    /// Gets active only categories as list view
    /// </summary>
    /// <returns>Collection of active category list views</returns>
    Task<IEnumerable<CategoryListView>> GetActiveOnlyListViewAsync();
    
    /// <summary>
    /// Gets sub-categories by parent ID as list view
    /// </summary>
    /// <param name="parentCategoryId">Parent category ID</param>
    /// <returns>Collection of sub-category list views</returns>
    Task<IEnumerable<CategoryListView>> GetSubCategoryListViewAsync(int parentCategoryId);

    // ============================================================
    // PAGINATION METHODS
    // ============================================================
    
    /// <summary>
    /// Gets paged list of categories with filtering and search
    /// </summary>
    /// <param name="page">Page number (1-indexed)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search keyword</param>
    /// <param name="sortBy">Sort column name</param>
    /// <param name="sortDescending">True for descending sort</param>
    /// <param name="filters">Additional filters (isActive, etc)</param>
    /// <returns>Paginated result with category list views</returns>
    Task<PaginatedResult<CategoryListView>> GetPagedListAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null);
    
    /// <summary>
    /// Gets category dropdown list (minimal data for selects)
    /// </summary>
    /// <returns>Collection of category dropdown views</returns>
    Task<IEnumerable<CategoryDropdownView>> GetDropdownListAsync();
}