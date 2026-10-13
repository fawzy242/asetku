using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Office;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Office;

namespace Whitebird.App.Features.Office;

/// <summary>
/// Service implementation for Office business logic
/// </summary>
public class OfficeService : BaseService, IOfficeService
{
    private readonly IGenericRepository<OfficeEntity> _repository;
    private readonly IOfficeReps _officeReps;
    private readonly IMasterDataService _masterDataService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public OfficeService(
        IGenericRepository<OfficeEntity> repository,
        IOfficeReps officeReps,
        IMasterDataService masterDataService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<OfficeService> logger) : base(logger)
    {
        _repository = repository;
        _officeReps = officeReps;
        _masterDataService = masterDataService;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    // ============================================================
    // BASIC CRUD
    // ============================================================

    public async Task<ServiceResult<OfficeDetailView>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var office = await _officeReps.GetDetailByIdAsync(id);
            if (office == null)
            {
                return ServiceResult<OfficeDetailView>.NotFound($"Office with id {id} not found");
            }
            return ServiceResult<OfficeDetailView>.Success(office);
        }, "get office by id");
    }

    public async Task<ServiceResult<IEnumerable<OfficeListView>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var offices = await _officeReps.GetAllListViewAsync();
            return ServiceResult<IEnumerable<OfficeListView>>.Success(offices);
        }, "get all offices");
    }

    public async Task<ServiceResult<OfficeDetailView>> CreateAsync(OfficeCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.OfficeName))
        {
            return ServiceResult<OfficeDetailView>.BadRequest("Office name is required");
        }

        var nameExists = await _officeReps.IsOfficeNameExistsAsync(model.OfficeName);
        if (nameExists)
        {
            return ServiceResult<OfficeDetailView>.Conflict($"Office '{model.OfficeName}' already exists");
        }

        if (!string.IsNullOrWhiteSpace(model.OfficeCode))
        {
            var codeExists = await _officeReps.IsOfficeCodeExistsAsync(model.OfficeCode);
            if (codeExists)
            {
                return ServiceResult<OfficeDetailView>.Conflict($"Office code '{model.OfficeCode}' already exists");
            }
        }

        if (model.ParentOfficeId.HasValue && model.ParentOfficeId.Value > 0)
        {
            var parentExists = await _officeReps.GetByIdRawAsync(model.ParentOfficeId.Value);
            if (parentExists == null)
            {
                return ServiceResult<OfficeDetailView>.BadRequest($"Parent office with id {model.ParentOfficeId} does not exist");
            }
        }

        if (model.OfficeType.HasValue)
        {
            var typeExists = await _masterDataService.GetValueAsync("OfficeType", model.OfficeType.Value);
            if (!typeExists.IsSuccess || typeExists.Data == null)
            {
                return ServiceResult<OfficeDetailView>.BadRequest($"Invalid office type: {model.OfficeType}");
            }
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<OfficeEntity>();
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);
            var created = await _officeReps.GetDetailByIdAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    TableNames.Office,
                    created.OfficeId,
                    $"Office '{created.OfficeName}' created successfully",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<OfficeDetailView>.Failure("Failed to retrieve created office")
                : ServiceResult<OfficeDetailView>.Success(created, "Office created successfully");
        }, "create office");
    }

    public async Task<ServiceResult<OfficeDetailView>> UpdateAsync(int id, OfficeUpdateViewModel model)
    {
        var nameExists = await _officeReps.IsOfficeNameExistsAsync(model.OfficeName, id);
        if (nameExists)
        {
            return ServiceResult<OfficeDetailView>.Conflict($"Office '{model.OfficeName}' already exists");
        }

        if (!string.IsNullOrWhiteSpace(model.OfficeCode))
        {
            var codeExists = await _officeReps.IsOfficeCodeExistsAsync(model.OfficeCode, id);
            if (codeExists)
            {
                return ServiceResult<OfficeDetailView>.Conflict($"Office code '{model.OfficeCode}' already exists");
            }
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _officeReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult<OfficeDetailView>.NotFound($"Office with id {id} not found");
            }

            if (model.ParentOfficeId.HasValue && model.ParentOfficeId.Value > 0)
            {
                if (model.ParentOfficeId.Value == id)
                {
                    return ServiceResult<OfficeDetailView>.BadRequest("Office cannot be parent of itself");
                }

                var parent = await _officeReps.GetByIdRawAsync(model.ParentOfficeId.Value);
                if (parent == null)
                {
                    return ServiceResult<OfficeDetailView>.BadRequest($"Parent office with id {model.ParentOfficeId} does not exist");
                }

                var children = await _officeReps.GetAllListViewAsync();
                if (children.Any(c => c.OfficeId == model.ParentOfficeId.Value && c.ParentOfficeId == id))
                {
                    return ServiceResult<OfficeDetailView>.BadRequest("Circular reference detected: child cannot become parent");
                }
            }

            var wasActive = existing.IsActive;
            var oldName = existing.OfficeName;
            var oldCode = existing.OfficeCode;
            var oldParentId = existing.ParentOfficeId;
            var oldOfficeType = existing.OfficeType;

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            // BR-01: if record was Active on entry, force Inactive after update.
            if (wasActive)
            {
                existing.IsActive = false;
            }

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
            {
                return ServiceResult<OfficeDetailView>.Failure("Failed to update office");
            }

            var updated = await _officeReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.Office,
                id,
                $"Office updated: Name '{oldName}' -> '{model.OfficeName}', Code '{oldCode}' -> '{model.OfficeCode}', ParentId '{oldParentId}' -> '{model.ParentOfficeId}', OfficeType '{oldOfficeType}' -> '{model.OfficeType}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<OfficeDetailView>.Success(updated!, "Office updated successfully");
        }, "update office");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _officeReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Office with id {id} not found");
            }

            var childCount = await _officeReps.GetChildCountAsync(id);
            if (childCount > 0)
            {
                return ServiceResult.BadRequest($"Cannot delete office with {childCount} sub-offices");
            }

            var result = await _repository.DeleteAsync(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(
                    TableNames.Office,
                    id,
                    $"Office '{existing.OfficeName}' deleted permanently",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to delete office")
                : ServiceResult.Success("Office deleted successfully");
        }, "delete office");
    }

    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _officeReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Office with id {id} not found");
            }

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogSoftDeleteAsync(
                    TableNames.Office,
                    id,
                    $"Office '{existing.OfficeName}' soft deleted",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to soft delete office")
                : ServiceResult.Success("Office soft deleted successfully");
        }, "soft delete office");
    }

    // ============================================================
    // GRID
    // ============================================================

    public async Task<ServiceResult<PaginatedResult<OfficeListView>>> GetGridDataAsync(OfficeGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _officeReps.GetPagedListAsync(request);
            return ServiceResult<PaginatedResult<OfficeListView>>.Success(result);
        }, "get office grid data");
    }

    // ============================================================
    // DROPDOWN
    // ============================================================

    public async Task<ServiceResult<IEnumerable<OfficeDropdownView>>> GetDropdownListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var offices = await _officeReps.GetDropdownListAsync();
            return ServiceResult<IEnumerable<OfficeDropdownView>>.Success(offices);
        }, "get office dropdown list");
    }

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    public async Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No office IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var activatedCount = 0;

            foreach (var id in request.Ids)
            {
                var office = await _officeReps.GetByIdRawAsync(id);
                if (office != null && !office.IsActive)
                {
                    office.IsActive = true;
                    office.ModifiedDate = DateTime.Now;
                    office.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(office);
                    activatedCount++;

                    await _activityLogService.LogUpdateAsync(
                        TableNames.Office,
                        id,
                        $"Office '{office.OfficeName}' activated via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            return ServiceResult<int>.Success(activatedCount,
                $"{activatedCount} office(s) activated successfully");
        }, "bulk activate offices");
    }

    public async Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No office IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var deactivatedCount = 0;

            foreach (var id in request.Ids)
            {
                var office = await _officeReps.GetByIdRawAsync(id);
                if (office != null && office.IsActive)
                {
                    var childCount = await _officeReps.GetChildCountAsync(id);
                    if (childCount > 0)
                    {
                        continue;
                    }

                    office.IsActive = false;
                    office.ModifiedDate = DateTime.Now;
                    office.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(office);
                    deactivatedCount++;

                    await _activityLogService.LogUpdateAsync(
                        TableNames.Office,
                        id,
                        $"Office '{office.OfficeName}' deactivated via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            var message = deactivatedCount > 0
                ? $"{deactivatedCount} office(s) deactivated successfully"
                : "No offices were deactivated. Offices with child offices cannot be deactivated.";

            return ServiceResult<int>.Success(deactivatedCount, message);
        }, "bulk deactivate offices");
    }

    public async Task<ServiceResult<int>> BulkDeleteAsync(BulkDeleteRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No office IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var deletedCount = 0;

            foreach (var id in request.Ids)
            {
                var office = await _officeReps.GetByIdRawAsync(id);
                if (office == null)
                {
                    continue;
                }

                // Reuse per-row business rule: refuse if it has child offices.
                var childCount = await _officeReps.GetChildCountAsync(id);
                if (childCount > 0)
                {
                    continue;
                }

                var result = await _repository.DeleteAsync(id);
                if (result > 0)
                {
                    deletedCount++;

                    await _activityLogService.LogDeleteAsync(
                        TableNames.Office,
                        id,
                        $"Office '{office.OfficeName}' deleted via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            var message = deletedCount > 0
                ? $"{deletedCount} office(s) deleted successfully"
                : "No offices were deleted. Offices with child offices cannot be deleted.";

            return ServiceResult<int>.Success(deletedCount, message);
        }, "bulk delete offices");
    }
}