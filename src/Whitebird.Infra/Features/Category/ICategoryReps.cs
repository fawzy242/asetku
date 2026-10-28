using Whitebird.Domain.Features.Category;
using Whitebird.Domain.Features.Common;

namespace Whitebird.Infra.Features.Category;

/// <summary>
/// Repository interface for Category operations
/// </summary>
public interface ICategoryReps
{
    // ============================================================
    // RAW ENTITY METHODS
    // ============================================================

    Task<CategoryEntity?> GetByIdRawAsync(int categoryId);
    Task<bool> IsCategoryNameExistsAsync(string categoryName, int? excludeCategoryId = null);
    Task<bool> IsCategoryCodeExistsAsync(string categoryCode, int? excludeCategoryId = null);
    Task<int> GetChildCountAsync(int categoryId);

    /// <summary>
    /// Counts the number of Asset rows whose CategoryId points at this category.
    /// Used as a hard-delete FK guard — an Asset row on the Category cannot be
    /// hard-deleted, regardless of the Asset's IsActive state.
    /// </summary>
    Task<int> GetAssetCountAsync(int categoryId);

    // ============================================================
    // DETAIL VIEW METHODS
    // ============================================================

    Task<CategoryDetailView?> GetDetailByIdAsync(int categoryId);
    Task<IEnumerable<CategoryListView>> GetAllListViewAsync();

    // ============================================================
    // PAGINATION
    // ============================================================

    Task<PaginatedResult<CategoryListView>> GetPagedListAsync(CategoryGridRequest request);

    // ============================================================
    // DROPDOWN
    // ============================================================

    Task<IEnumerable<CategoryDropdownView>> GetDropdownListAsync();
}