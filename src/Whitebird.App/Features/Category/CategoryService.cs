using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Category;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Category;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Category;

public class CategoryService
    : CommonService<CategoryEntity, CategoryDetailView, CategoryListView, CategoryGridRequest, CategoryCreateViewModel, CategoryUpdateViewModel>,
      ICategoryService
{
    private readonly ICategoryReps _categoryReps;

    public CategoryService(
        ICommonReps repository,
        ICategoryReps categoryReps,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<CategoryService> logger)
        : base(repository, currentUserService, activityLogService, logger)
    {
        _categoryReps = categoryReps;
    }

    protected override string EntityLabel => "Category";
    protected override string EntityLabelPlural => "Categories";
    protected override string TableName => TableNames.Category;

    protected override string BulkDeactivateEmptyMessage =>
        "No categories were deactivated. Categories with subcategories cannot be deactivated.";
    protected override string BulkDeleteEmptyMessage =>
        "No categories were deleted. Categories with subcategories or associated assets cannot be deleted.";

    protected override async Task<CategoryDetailView?> LoadDetailAsync(int id) =>
        await _categoryReps.GetDetailByIdAsync(id);

    protected override async Task<IEnumerable<CategoryListView>> LoadAllAsync() =>
        await _categoryReps.GetAllListViewAsync();

    protected override async Task<PaginatedResult<CategoryListView>> LoadPageAsync(CategoryGridRequest request) =>
        await _categoryReps.GetPagedListAsync(request);

    protected override async Task<ServiceResult<CategoryDetailView>?> ValidateCreateAsync(CategoryCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.CategoryName))
            return ServiceResult<CategoryDetailView>.BadRequest("Category name is required");

        if (await _categoryReps.IsCategoryNameExistsAsync(model.CategoryName))
            return ServiceResult<CategoryDetailView>.Conflict($"Category '{model.CategoryName}' already exists");

        if (!string.IsNullOrWhiteSpace(model.CategoryCode) &&
            await _categoryReps.IsCategoryCodeExistsAsync(model.CategoryCode))
            return ServiceResult<CategoryDetailView>.Conflict($"Category code '{model.CategoryCode}' already exists");

        if (model.ParentCategoryId.HasValue && model.ParentCategoryId.Value > 0)
        {
            if (await _categoryReps.GetByIdRawAsync(model.ParentCategoryId.Value) == null)
                return ServiceResult<CategoryDetailView>.BadRequest(
                    $"Parent category with id {model.ParentCategoryId} does not exist");
        }

        return null;
    }

    protected override async Task<ServiceResult<CategoryDetailView>?> ValidateUpdateAsync(int id, CategoryUpdateViewModel model)
    {
        if (model.ParentCategoryId.HasValue && model.ParentCategoryId.Value > 0)
        {
            if (model.ParentCategoryId.Value == id)
                return ServiceResult<CategoryDetailView>.BadRequest("Category cannot be parent of itself");

            if (await _categoryReps.GetByIdRawAsync(model.ParentCategoryId.Value) == null)
                return ServiceResult<CategoryDetailView>.BadRequest(
                    $"Parent category with id {model.ParentCategoryId} does not exist");
        }

        if (await _categoryReps.IsCategoryNameExistsAsync(model.CategoryName, id))
            return ServiceResult<CategoryDetailView>.Conflict($"Category '{model.CategoryName}' already exists");

        if (!string.IsNullOrWhiteSpace(model.CategoryCode) &&
            await _categoryReps.IsCategoryCodeExistsAsync(model.CategoryCode, id))
            return ServiceResult<CategoryDetailView>.Conflict($"Category code '{model.CategoryCode}' already exists");

        return null;
    }

    protected override void MapCreate(CategoryCreateViewModel model, CategoryEntity entity) => model.Adapt(entity);
    protected override void MapUpdate(CategoryUpdateViewModel model, CategoryEntity entity) => model.Adapt(entity);
    protected override string Describe(CategoryEntity entity) => entity.CategoryName;

    protected override async Task<(bool CanProceed, string? Reason)> CanDeleteAsync(int id)
    {
        var childCount = await _categoryReps.GetChildCountAsync(id);
        if (childCount > 0)
            return (false, $"Cannot delete category with {childCount} subcategories");

        var assetCount = await _categoryReps.GetAssetCountAsync(id);
        if (assetCount > 0)
            return (false, $"Cannot delete category with {assetCount} associated asset(s)");

        return (true, (string?)null);
    }

    protected override async Task<(bool CanProceed, string? Reason)> CanDeactivateAsync(int id)
    {
        var childCount = await _categoryReps.GetChildCountAsync(id);
        return childCount > 0 ? (false, (string?)null) : (true, (string?)null);
    }

    public async Task<ServiceResult<IEnumerable<CategoryDropdownView>>> GetDropdownListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var categories = await _categoryReps.GetDropdownListAsync();
            return ServiceResult<IEnumerable<CategoryDropdownView>>.Success(categories);
        }, "get category dropdown list");
    }
}