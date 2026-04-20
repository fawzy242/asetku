using Whitebird.Domain.Features.Category.View;
using Whitebird.App.Features.Common.Service;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Category.Interfaces;

public interface ICategoryService
{
    Task<ServiceResult<CategoryDetailViewModel>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<CategoryListViewModel>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<CategoryListViewModel>>> GetActiveOnlyAsync();
    Task<ServiceResult<IEnumerable<CategoryListViewModel>>> GetSubCategoriesAsync(int parentId);
    Task<ServiceResult<CategoryDetailViewModel>> CreateAsync(CategoryCreateViewModel model);
    Task<ServiceResult<CategoryDetailViewModel>> UpdateAsync(int id, CategoryUpdateViewModel model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);
    Task<ServiceResult<PaginatedResult<CategoryListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null);
}