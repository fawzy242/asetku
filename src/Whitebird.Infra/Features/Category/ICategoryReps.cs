using Whitebird.Domain.Features.Category;

namespace Whitebird.Infra.Features.Category;

public interface ICategoryReps
{
    Task<CategoryEntity?> GetByIdRawAsync(int categoryId);
    Task<CategoryEntity?> GetByIdWithRelationsAsync(int categoryId);
    Task<IEnumerable<CategoryEntity>> GetAllWithRelationsAsync();
    Task<IEnumerable<CategoryEntity>> GetActiveOnlyWithRelationsAsync();
    Task<IEnumerable<CategoryEntity>> GetSubCategoryAsync(int parentCategoryId);
    Task<bool> IsCategoryNameExistsAsync(string categoryName, int? excludeCategoryId = null);
    Task<bool> IsCategoryCodeExistsAsync(string categoryCode, int? excludeCategoryId = null); // NEW
    Task<int> GetChildCountAsync(int categoryId);
}