using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Category.Interfaces;
using Whitebird.App.Features.Common.Service;
using Whitebird.Domain.Features.Category.Entities;
using Whitebird.Domain.Features.Category.View;
using Whitebird.Infra.Features.Category;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Category.Service;

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

    public async Task<ServiceResult<CategoryDetailViewModel>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var category = await _categoryReps.GetByIdWithRelationsAsync(id);
            if (category == null)
                return ServiceResult<CategoryDetailViewModel>.NotFound($"Category with id {id} not found");

            var viewModel = category.Adapt<CategoryDetailViewModel>();
            viewModel.ChildCount = await _categoryReps.GetChildCountAsync(id);
            return ServiceResult<CategoryDetailViewModel>.Success(viewModel);
        }, "get category by id");
    }

    public async Task<ServiceResult<IEnumerable<CategoryListViewModel>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var categories = await _categoryReps.GetAllWithRelationsAsync();
            return ServiceResult<IEnumerable<CategoryListViewModel>>.Success(categories.Adapt<IEnumerable<CategoryListViewModel>>());
        }, "get all categories");
    }

    public async Task<ServiceResult<IEnumerable<CategoryListViewModel>>> GetActiveOnlyAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var categories = await _categoryReps.GetActiveOnlyWithRelationsAsync();
            return ServiceResult<IEnumerable<CategoryListViewModel>>.Success(categories.Adapt<IEnumerable<CategoryListViewModel>>());
        }, "get active categories");
    }

    public async Task<ServiceResult<IEnumerable<CategoryListViewModel>>> GetSubCategoriesAsync(int parentId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var categories = await _categoryReps.GetSubCategoryAsync(parentId);
            return ServiceResult<IEnumerable<CategoryListViewModel>>.Success(categories.Adapt<IEnumerable<CategoryListViewModel>>());
        }, "get subcategories");
    }

    public async Task<ServiceResult<CategoryDetailViewModel>> CreateAsync(CategoryCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.CategoryName))
            return ServiceResult<CategoryDetailViewModel>.BadRequest("Category name is required");

        if (await _categoryReps.IsCategoryNameExistsAsync(model.CategoryName))
            return ServiceResult<CategoryDetailViewModel>.Conflict($"Category '{model.CategoryName}' already exists");

        if (model.ParentCategoryId.HasValue && model.ParentCategoryId.Value > 0)
        {
            var parentExists = await _categoryReps.GetByIdRawAsync(model.ParentCategoryId.Value);
            if (parentExists == null)
                return ServiceResult<CategoryDetailViewModel>.BadRequest($"Parent category with id {model.ParentCategoryId} does not exist");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<CategoryEntity>();
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);
            entity.CategoryId = Convert.ToInt32(id);

            var viewModel = entity.Adapt<CategoryDetailViewModel>();

            if (model.ParentCategoryId.HasValue && model.ParentCategoryId.Value > 0)
            {
                var parent = await _categoryReps.GetByIdRawAsync(model.ParentCategoryId.Value);
                viewModel.ParentCategoryName = parent?.CategoryName;
            }

            await _activityLogService.LogCreateAsync(
                "Category",
                entity.CategoryId,
                $"Category '{entity.CategoryName}' created successfully",
                _currentUserService.GetDisplayName());

            return ServiceResult<CategoryDetailViewModel>.Success(viewModel, "Category created successfully");
        }, "create category", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Category", 0, "Create Category", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<CategoryDetailViewModel>> UpdateAsync(int id, CategoryUpdateViewModel model)
    {
        if (model.ParentCategoryId.HasValue && model.ParentCategoryId.Value > 0)
        {
            if (model.ParentCategoryId.Value == id)
                return ServiceResult<CategoryDetailViewModel>.BadRequest("Category cannot be parent of itself");

            var parent = await _categoryReps.GetByIdRawAsync(model.ParentCategoryId.Value);
            if (parent == null)
                return ServiceResult<CategoryDetailViewModel>.BadRequest($"Parent category with id {model.ParentCategoryId} does not exist");
        }

        if (await _categoryReps.IsCategoryNameExistsAsync(model.CategoryName, id))
            return ServiceResult<CategoryDetailViewModel>.Conflict($"Category '{model.CategoryName}' already exists");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _categoryReps.GetByIdRawAsync(id);
            if (existing == null)
                return ServiceResult<CategoryDetailViewModel>.NotFound($"Category with id {id} not found");

            var oldName = existing.CategoryName;
            var oldParentId = existing.ParentCategoryId;

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
                return ServiceResult<CategoryDetailViewModel>.Failure("Failed to update category");

            var updated = await _categoryReps.GetByIdWithRelationsAsync(id);

            await _activityLogService.LogUpdateAsync(
                "Category",
                id,
                $"Category updated: Name '{oldName}' -> '{existing.CategoryName}', ParentId '{oldParentId}' -> '{existing.ParentCategoryId}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<CategoryDetailViewModel>.Success(updated!.Adapt<CategoryDetailViewModel>(), "Category updated successfully");
        }, "update category", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Category", id, "Update Category", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _categoryReps.GetByIdRawAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Category with id {id} not found");

            if (await _categoryReps.GetChildCountAsync(id) > 0)
                return ServiceResult.BadRequest("Cannot delete category with subcategories");

            var result = await _repository.DeleteAsync(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(
                    "Category",
                    id,
                    $"Category '{existing.CategoryName}' deleted permanently",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to delete category")
                : ServiceResult.Success("Category deleted successfully");
        }, "delete category", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Category", id, "Delete Category", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _categoryReps.GetByIdRawAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Category with id {id} not found");

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogSoftDeleteAsync(
                    "Category",
                    id,
                    $"Category '{existing.CategoryName}' soft deleted",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to soft delete category")
                : ServiceResult.Success("Category soft deleted successfully");
        }, "soft delete category", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Category", id, "Soft Delete Category", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<PaginatedResult<CategoryListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var categories = await _categoryReps.GetAllWithRelationsAsync();
            var query = categories.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (c.Description != null && c.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                );
            }

            var totalCount = query.Count();
            var pagedData = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var viewModels = pagedData.Adapt<List<CategoryListViewModel>>();

            return ServiceResult<PaginatedResult<CategoryListViewModel>>.Success(new PaginatedResult<CategoryListViewModel>
            {
                Data = viewModels,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }, "get category grid data");
    }
}