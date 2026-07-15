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

    // ============================================================
    // BASIC CRUD
    // ============================================================

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

    public async Task<ServiceResult<IEnumerable<CategoryListView>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var categories = await _categoryReps.GetAllListViewAsync();
            return ServiceResult<IEnumerable<CategoryListView>>.Success(categories);
        }, "get all categories");
    }

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
            var created = await _categoryReps.GetDetailByIdAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    TableNames.Category,
                    created.CategoryId,
                    $"Category '{created.CategoryName}' created successfully",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<CategoryDetailView>.Failure("Failed to retrieve created category")
                : ServiceResult<CategoryDetailView>.Success(created, "Category created successfully");
        }, "create category");
    }

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
                $"Category updated: Name '{oldName}' -> '{model.CategoryName}', ParentId '{oldParentId}' -> '{model.ParentCategoryId}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<CategoryDetailView>.Success(updated!, "Category updated successfully");
        }, "update category");
    }

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
        }, "delete category");
    }

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
        }, "soft delete category");
    }

    // ============================================================
    // GRID
    // ============================================================

    public async Task<ServiceResult<PaginatedResult<CategoryListView>>> GetGridDataAsync(CategoryGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _categoryReps.GetPagedListAsync(request);
            return ServiceResult<PaginatedResult<CategoryListView>>.Success(result);
        }, "get category grid data");
    }

    // ============================================================
    // DROPDOWN
    // ============================================================

    public async Task<ServiceResult<IEnumerable<CategoryDropdownView>>> GetDropdownListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var categories = await _categoryReps.GetDropdownListAsync();
            return ServiceResult<IEnumerable<CategoryDropdownView>>.Success(categories);
        }, "get category dropdown list");
    }

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    public async Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No category IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var activatedCount = 0;

            foreach (var id in request.Ids)
            {
                var category = await _categoryReps.GetByIdRawAsync(id);
                if (category != null && !category.IsActive)
                {
                    category.IsActive = true;
                    category.ModifiedDate = DateTime.Now;
                    category.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(category);
                    activatedCount++;

                    await _activityLogService.LogUpdateAsync(
                        TableNames.Category,
                        id,
                        $"Category '{category.CategoryName}' activated via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            return ServiceResult<int>.Success(activatedCount,
                $"{activatedCount} category(s) activated successfully");
        }, "bulk activate categories");
    }

    public async Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No category IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var deactivatedCount = 0;

            foreach (var id in request.Ids)
            {
                var category = await _categoryReps.GetByIdRawAsync(id);
                if (category != null && category.IsActive)
                {
                    var childCount = await _categoryReps.GetChildCountAsync(id);
                    if (childCount > 0)
                    {
                        continue;
                    }

                    category.IsActive = false;
                    category.ModifiedDate = DateTime.Now;
                    category.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(category);
                    deactivatedCount++;

                    await _activityLogService.LogUpdateAsync(
                        TableNames.Category,
                        id,
                        $"Category '{category.CategoryName}' deactivated via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            var message = deactivatedCount > 0
                ? $"{deactivatedCount} category(s) deactivated successfully"
                : "No categories were deactivated. Categories with subcategories cannot be deactivated.";

            return ServiceResult<int>.Success(deactivatedCount, message);
        }, "bulk deactivate categories");
    }
}