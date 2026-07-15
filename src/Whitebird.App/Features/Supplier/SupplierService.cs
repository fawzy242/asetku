using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Supplier;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Supplier;

namespace Whitebird.App.Features.Supplier;

/// <summary>
/// Service implementation for Supplier business logic
/// </summary>
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

    // ============================================================
    // BASIC CRUD
    // ============================================================

    public async Task<ServiceResult<SupplierDetailView>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var supplier = await _supplierReps.GetDetailByIdAsync(id);
            if (supplier == null)
            {
                return ServiceResult<SupplierDetailView>.NotFound($"Supplier with id {id} not found");
            }
            return ServiceResult<SupplierDetailView>.Success(supplier);
        }, "get supplier by id");
    }

    public async Task<ServiceResult<IEnumerable<SupplierListView>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var suppliers = await _supplierReps.GetAllListViewAsync();
            return ServiceResult<IEnumerable<SupplierListView>>.Success(suppliers);
        }, "get all suppliers");
    }

    public async Task<ServiceResult<SupplierDetailView>> CreateAsync(SupplierCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.SupplierName))
        {
            return ServiceResult<SupplierDetailView>.BadRequest("Supplier name is required");
        }

        var nameExists = await _supplierReps.IsSupplierNameExistsAsync(model.SupplierName);
        if (nameExists)
        {
            return ServiceResult<SupplierDetailView>.Conflict($"Supplier '{model.SupplierName}' already exists");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<SupplierEntity>();
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);
            var created = await _supplierReps.GetDetailByIdAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    TableNames.Supplier,
                    created.SupplierId,
                    $"Supplier '{created.SupplierName}' created successfully",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<SupplierDetailView>.Failure("Failed to retrieve created supplier")
                : ServiceResult<SupplierDetailView>.Success(created, "Supplier created successfully");
        }, "create supplier");
    }

    public async Task<ServiceResult<SupplierDetailView>> UpdateAsync(int id, SupplierUpdateViewModel model)
    {
        var nameExists = await _supplierReps.IsSupplierNameExistsAsync(model.SupplierName, id);
        if (nameExists)
        {
            return ServiceResult<SupplierDetailView>.Conflict($"Supplier '{model.SupplierName}' already exists");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _supplierReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult<SupplierDetailView>.NotFound($"Supplier with id {id} not found");
            }

            var oldName = existing.SupplierName;
            var oldContact = existing.ContactPerson;

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
            {
                return ServiceResult<SupplierDetailView>.Failure("Failed to update supplier");
            }

            var updated = await _supplierReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.Supplier,
                id,
                $"Supplier updated: Name '{oldName}' -> '{model.SupplierName}', Contact '{oldContact}' -> '{model.ContactPerson}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<SupplierDetailView>.Success(updated!, "Supplier updated successfully");
        }, "update supplier");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _supplierReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Supplier with id {id} not found");
            }

            var assetCount = await _supplierReps.GetAssetCountAsync(id);
            if (assetCount > 0)
            {
                return ServiceResult.BadRequest($"Cannot delete supplier with {assetCount} associated assets");
            }

            var result = await _repository.DeleteAsync(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(
                    TableNames.Supplier,
                    id,
                    $"Supplier '{existing.SupplierName}' deleted permanently",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to delete supplier")
                : ServiceResult.Success("Supplier deleted successfully");
        }, "delete supplier");
    }

    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _supplierReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Supplier with id {id} not found");
            }

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogSoftDeleteAsync(
                    TableNames.Supplier,
                    id,
                    $"Supplier '{existing.SupplierName}' soft deleted",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to soft delete supplier")
                : ServiceResult.Success("Supplier soft deleted successfully");
        }, "soft delete supplier");
    }

    // ============================================================
    // GRID
    // ============================================================

    public async Task<ServiceResult<PaginatedResult<SupplierListView>>> GetGridDataAsync(SupplierGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _supplierReps.GetPagedListAsync(request);
            return ServiceResult<PaginatedResult<SupplierListView>>.Success(result);
        }, "get supplier grid data");
    }

    // ============================================================
    // DROPDOWN
    // ============================================================

    public async Task<ServiceResult<IEnumerable<SupplierDropdownView>>> GetDropdownListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var suppliers = await _supplierReps.GetDropdownListAsync();
            return ServiceResult<IEnumerable<SupplierDropdownView>>.Success(suppliers);
        }, "get supplier dropdown list");
    }

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    public async Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No supplier IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var activatedCount = 0;

            foreach (var id in request.Ids)
            {
                var supplier = await _supplierReps.GetByIdRawAsync(id);
                if (supplier != null && !supplier.IsActive)
                {
                    supplier.IsActive = true;
                    supplier.ModifiedDate = DateTime.Now;
                    supplier.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(supplier);
                    activatedCount++;

                    await _activityLogService.LogUpdateAsync(
                        TableNames.Supplier,
                        id,
                        $"Supplier '{supplier.SupplierName}' activated via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            return ServiceResult<int>.Success(activatedCount,
                $"{activatedCount} supplier(s) activated successfully");
        }, "bulk activate suppliers");
    }

    public async Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No supplier IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var deactivatedCount = 0;

            foreach (var id in request.Ids)
            {
                var supplier = await _supplierReps.GetByIdRawAsync(id);
                if (supplier != null && supplier.IsActive)
                {
                    var assetCount = await _supplierReps.GetAssetCountAsync(id);
                    if (assetCount > 0)
                    {
                        continue;
                    }

                    supplier.IsActive = false;
                    supplier.ModifiedDate = DateTime.Now;
                    supplier.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(supplier);
                    deactivatedCount++;

                    await _activityLogService.LogUpdateAsync(
                        TableNames.Supplier,
                        id,
                        $"Supplier '{supplier.SupplierName}' deactivated via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            var message = deactivatedCount > 0
                ? $"{deactivatedCount} supplier(s) deactivated successfully"
                : "No suppliers were deactivated. Suppliers with associated assets cannot be deactivated.";

            return ServiceResult<int>.Success(deactivatedCount, message);
        }, "bulk deactivate suppliers");
    }
}