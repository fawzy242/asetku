using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Office;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.MasterData;
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
    private readonly IMasterDataLookupService _masterDataLookupService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public OfficeService(
        IGenericRepository<OfficeEntity> repository,
        IOfficeReps officeReps,
        IMasterDataService masterDataService,
        IMasterDataLookupService masterDataLookupService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<OfficeService> logger) : base(logger)
    {
        _repository = repository;
        _officeReps = officeReps;
        _masterDataService = masterDataService;
        _masterDataLookupService = masterDataLookupService;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<OfficeListView>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var offices = await _officeReps.GetAllListViewAsync();
            return ServiceResult<IEnumerable<OfficeListView>>.Success(offices);
        }, "get all offices");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<OfficeListView>>> GetActiveOnlyAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var offices = await _officeReps.GetActiveOnlyListViewAsync();
            return ServiceResult<IEnumerable<OfficeListView>>.Success(offices);
        }, "get active offices");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<OfficeListView>>> GetSubOfficesAsync(int parentId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var parent = await _officeReps.GetByIdRawAsync(parentId);
            if (parent == null)
            {
                return ServiceResult<IEnumerable<OfficeListView>>.NotFound($"Parent office with id {parentId} not found");
            }

            var offices = await _officeReps.GetSubOfficesListViewAsync(parentId);
            return ServiceResult<IEnumerable<OfficeListView>>.Success(offices);
        }, "get sub offices");
    }

    /// <inheritdoc />
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
            var typeExists = await _masterDataLookupService.GetOfficeTypeNameAsync(model.OfficeType.Value);
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
        }, "create office", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Office, 0, "Create Office", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
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

                var children = await _officeReps.GetSubOfficesListViewAsync(id);
                if (children.Any(c => c.OfficeId == model.ParentOfficeId.Value))
                {
                    return ServiceResult<OfficeDetailView>.BadRequest("Circular reference detected: child cannot become parent");
                }
            }

            var oldName = existing.OfficeName;
            var oldCode = existing.OfficeCode;
            var oldParentId = existing.ParentOfficeId;
            var oldOfficeType = existing.OfficeType;

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

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
        }, "update office", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Office, id, "Update Office", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
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
        }, "delete office", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Office, id, "Delete Office", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
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
        }, "soft delete office", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.Office, id, "Soft Delete Office", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PaginatedResult<OfficeListView>>> GetGridDataAsync(
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

            var result = await _officeReps.GetPagedListAsync(page, pageSize, search, "OfficeName", false, filters);
            return ServiceResult<PaginatedResult<OfficeListView>>.Success(result);
        }, "get office grid data");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<OfficeDropdownView>>> GetDropdownListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var offices = await _officeReps.GetDropdownListAsync();
            return ServiceResult<IEnumerable<OfficeDropdownView>>.Success(offices);
        }, "get office dropdown list");
    }
}