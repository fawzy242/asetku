using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Supplier.Interfaces;
using Whitebird.App.Features.Common.Service;
using Whitebird.Domain.Features.Supplier.Entities;
using Whitebird.Domain.Features.Supplier.View;
using Whitebird.Infra.Features.Supplier;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Supplier.Service;

public class SupplierService : BaseService, ISupplierService
{
    private readonly IGenericRepository<SupplierEntity> _repository;
    private readonly ISupplierReps _supplierReps;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public SupplierService(
        IGenericRepository<SupplierEntity> repository,
        ISupplierReps supplierReps,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<SupplierService> logger) : base(logger)
    {
        _repository = repository;
        _supplierReps = supplierReps;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<ServiceResult<SupplierDetailViewModel>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var supplier = await _supplierReps.GetByIdAsync(id);
            if (supplier == null)
                return ServiceResult<SupplierDetailViewModel>.NotFound($"Supplier with id {id} not found");

            var viewModel = supplier.Adapt<SupplierDetailViewModel>();
            viewModel.AssetCount = await _supplierReps.GetAssetCountAsync(id);
            return ServiceResult<SupplierDetailViewModel>.Success(viewModel);
        }, "get supplier by id");
    }

    public async Task<ServiceResult<IEnumerable<SupplierListViewModel>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var suppliers = await _supplierReps.GetAllAsync();
            return ServiceResult<IEnumerable<SupplierListViewModel>>.Success(suppliers.Adapt<IEnumerable<SupplierListViewModel>>());
        }, "get all suppliers");
    }

    public async Task<ServiceResult<IEnumerable<SupplierListViewModel>>> GetActiveOnlyAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var suppliers = await _supplierReps.GetActiveOnlyAsync();
            return ServiceResult<IEnumerable<SupplierListViewModel>>.Success(suppliers.Adapt<IEnumerable<SupplierListViewModel>>());
        }, "get active suppliers");
    }

    public async Task<ServiceResult<SupplierDetailViewModel>> CreateAsync(SupplierCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.SupplierName))
            return ServiceResult<SupplierDetailViewModel>.BadRequest("Supplier name is required");

        if (await _supplierReps.IsSupplierNameExistsAsync(model.SupplierName))
            return ServiceResult<SupplierDetailViewModel>.Conflict($"Supplier '{model.SupplierName}' already exists");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<SupplierEntity>();
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);
            var created = await _supplierReps.GetByIdAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    "Supplier",
                    created.SupplierId,
                    $"Supplier '{created.SupplierName}' created successfully",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<SupplierDetailViewModel>.Failure("Failed to retrieve created supplier")
                : ServiceResult<SupplierDetailViewModel>.Success(created.Adapt<SupplierDetailViewModel>(), "Supplier created successfully");
        }, "create supplier", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Supplier", 0, "Create Supplier", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<SupplierDetailViewModel>> UpdateAsync(int id, SupplierUpdateViewModel model)
    {
        if (await _supplierReps.IsSupplierNameExistsAsync(model.SupplierName, id))
            return ServiceResult<SupplierDetailViewModel>.Conflict($"Supplier '{model.SupplierName}' already exists");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult<SupplierDetailViewModel>.NotFound($"Supplier with id {id} not found");

            var oldName = existing.SupplierName;
            var oldContact = existing.ContactPerson;

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
                return ServiceResult<SupplierDetailViewModel>.Failure("Failed to update supplier");

            var updated = await _supplierReps.GetByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                "Supplier",
                id,
                $"Supplier updated: Name '{oldName}' -> '{existing.SupplierName}', Contact '{oldContact}' -> '{existing.ContactPerson}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<SupplierDetailViewModel>.Success(updated!.Adapt<SupplierDetailViewModel>(), "Supplier updated successfully");
        }, "update supplier", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Supplier", id, "Update Supplier", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Supplier with id {id} not found");

            if (await _supplierReps.GetAssetCountAsync(id) > 0)
                return ServiceResult.BadRequest("Cannot delete supplier with associated assets");

            var result = await _repository.DeleteAsync(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(
                    "Supplier",
                    id,
                    $"Supplier '{existing.SupplierName}' deleted permanently",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to delete supplier")
                : ServiceResult.Success("Supplier deleted successfully");
        }, "delete supplier", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Supplier", id, "Delete Supplier", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Supplier with id {id} not found");

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogSoftDeleteAsync(
                    "Supplier",
                    id,
                    $"Supplier '{existing.SupplierName}' soft deleted",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to soft delete supplier")
                : ServiceResult.Success("Supplier soft deleted successfully");
        }, "soft delete supplier", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Supplier", id, "Soft Delete Supplier", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<PaginatedResult<SupplierListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var suppliers = await _supplierReps.GetAllAsync();
            var query = suppliers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.SupplierName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (s.ContactPerson != null && s.ContactPerson.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (s.Email != null && s.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
                );
            }

            var totalCount = query.Count();
            var pagedData = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var viewModels = pagedData.Adapt<List<SupplierListViewModel>>();

            return ServiceResult<PaginatedResult<SupplierListViewModel>>.Success(new PaginatedResult<SupplierListViewModel>
            {
                Data = viewModels,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }, "get supplier grid data");
    }
}