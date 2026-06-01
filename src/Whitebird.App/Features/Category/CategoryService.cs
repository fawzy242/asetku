using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Category;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Category;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Category;

/// <summary>
/// Service implementation for Category business logic
/// </summary>
public class CategoryService : BaseService, ICategoryService
{
    private readonly IGenericRepository<CategoryEntity> _repository;
    private readonly ICategoryReps _categoryReps;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public CategoryService(
        IGenericRepository<CategoryEntity> repository,
        ICategoryReps categoryReps,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<CategoryService> logger) : base(logger)
    {
        _repository = repository;
        _categoryReps = categoryReps;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<CategoryDetailView>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var category = await _categoryReps.GetDetailByIdAsync(id);
            if (category == null)
            {
                return ServiceResult<CategoryDetailView>.NotFound($"Category with id {id} not found");
            }
            return ServiceResult<CategoryDetailView>.Success(category);
        }, "get category by id");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<CategoryListView>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var categories = await _categoryReps.GetAllListViewAsync();
            return ServiceResult<IEnumerable<CategoryListView>>.Success(categories);
        }, "get all categories");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<CategoryListView>>> GetActiveOnlyAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var categories = await _categoryReps.GetActiveOnlyListViewAsync();
            return ServiceResult<IEnumerable<CategoryListView>>.Success(categories);
        }, "get active categories");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<CategoryListView>>> GetSubCategoriesAsync(int parentId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var parent = await _categoryReps.GetByIdRawAsync(parentId);
            if (parent == null)
            {
                return ServiceResult<IEnumerable<CategoryListView>>.NotFound($"Parent category with id {parentId} not found");
            }

            var categories = await _categoryReps.GetSubCategoryListViewAsync(parentId);
            return ServiceResult<IEnumerable<CategoryListView>>.Success(categories);
        }, "get subcategories");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<CategoryDetailView>> CreateAsync(CategoryCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.CategoryName))
        {
            return ServiceResult<CategoryDetailView>.BadRequest("Category name is required");
        }

        var nameExists = await _categoryReps.IsCategoryNameExistsAsync(model.CategoryName);
        if (nameExists)
        {
            return ServiceResult<CategoryDetailView>.Conflict($"Category '{model.CategoryName}' already exists");
        }

        if (!string.IsNullOrWhiteSpace(model.CategoryCode))
        {
            var codeExists = await _categoryReps.IsCategoryCodeExistsAsync(model.CategoryCode);
            if (codeExists)
            {
                return ServiceResult<CategoryDetailView>.Conflict($"Category code '{model.CategoryCode}' already exists");
            }
        }

        if (model.ParentCategoryId.HasValue && model.ParentCategoryId.Value > 0)
        {
            var parentExists = await _categoryReps.GetByIdRawAsync(model.ParentCategoryId.Value);
            if (parentExists == null)
            {
                return ServiceResult<CategoryDetailView>.BadRequest($"Parent category with id {model.ParentCategoryId} does not exist");
            }
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<CategoryEntity>();
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);
            entity.CategoryId = Convert.ToInt32(id);

            var created = await _categoryReps.GetDetailByIdAsync(entity.CategoryId);

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    TableNames.Category,
                    entity.CategoryId,
                    $"Category '{entity.CategoryName}' created successfully",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<CategoryDetailView>.Failure("Failed to retrieve created category")
                : ServiceResult<CategoryDetailView>.Success(created, "Category created successfully");
        }, "create category", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Category, 0, "Create Category", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult<CategoryDetailView>> UpdateAsync(int id, CategoryUpdateViewModel model)
    {
        if (model.ParentCategoryId.HasValue && model.ParentCategoryId.Value > 0)
        {
            if (model.ParentCategoryId.Value == id)
            {
                return ServiceResult<CategoryDetailView>.BadRequest("Category cannot be parent of itself");
            }

            var parent = await _categoryReps.GetByIdRawAsync(model.ParentCategoryId.Value);
            if (parent == null)
            {
                return ServiceResult<CategoryDetailView>.BadRequest($"Parent category with id {model.ParentCategoryId} does not exist");
            }
        }

        var nameExists = await _categoryReps.IsCategoryNameExistsAsync(model.CategoryName, id);
        if (nameExists)
        {
            return ServiceResult<CategoryDetailView>.Conflict($"Category '{model.CategoryName}' already exists");
        }

        if (!string.IsNullOrWhiteSpace(model.CategoryCode))
        {
            var codeExists = await _categoryReps.IsCategoryCodeExistsAsync(model.CategoryCode, id);
            if (codeExists)
            {
                return ServiceResult<CategoryDetailView>.Conflict($"Category code '{model.CategoryCode}' already exists");
            }
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _categoryReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult<CategoryDetailView>.NotFound($"Category with id {id} not found");
            }

            var oldName = existing.CategoryName;
            var oldParentId = existing.ParentCategoryId;

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
            {
                return ServiceResult<CategoryDetailView>.Failure("Failed to update category");
            }

            var updated = await _categoryReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.Category,
                id,
                $"Category updated: Name '{oldName}' -> '{existing.CategoryName}', ParentId '{oldParentId}' -> '{existing.ParentCategoryId}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<CategoryDetailView>.Success(updated!, "Category updated successfully");
        }, "update category", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Category, id, "Update Category", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _categoryReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Category with id {id} not found");
            }

            var childCount = await _categoryReps.GetChildCountAsync(id);
            if (childCount > 0)
            {
                return ServiceResult.BadRequest($"Cannot delete category with {childCount} subcategories");
            }

            var result = await _repository.DeleteAsync(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(
                    TableNames.Category,
                    id,
                    $"Category '{existing.CategoryName}' deleted permanently",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to delete category")
                : ServiceResult.Success("Category deleted successfully");
        }, "delete category", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Category, id, "Delete Category", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _categoryReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Category with id {id} not found");
            }

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogSoftDeleteAsync(
                    TableNames.Category,
                    id,
                    $"Category '{existing.CategoryName}' soft deleted",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to soft delete category")
                : ServiceResult.Success("Category soft deleted successfully");
        }, "soft delete category", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Category, id, "Soft Delete Category", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PaginatedResult<CategoryListView>>> GetGridDataAsync(
        int page, int pageSize, string? search = null, bool? isActive = null, Dictionary<string, object>? filters = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            if (filters == null)
            {
                filters = new Dictionary<string, object>();
            }
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                filters["search"] = search;
            }
            
            if (isActive.HasValue)
            {
                filters["isActive"] = isActive.Value;
            }

            var result = await _categoryReps.GetPagedListAsync(page, pageSize, search, "CategoryName", false, filters);
            return ServiceResult<PaginatedResult<CategoryListView>>.Success(result);
        }, "get category grid data");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<CategoryDropdownView>>> GetDropdownListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var categories = await _categoryReps.GetDropdownListAsync();
            return ServiceResult<IEnumerable<CategoryDropdownView>>.Success(categories);
        }, "get category dropdown list");
    }
}