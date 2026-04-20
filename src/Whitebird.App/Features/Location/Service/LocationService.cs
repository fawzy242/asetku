using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Location.Interfaces;
using Whitebird.App.Features.Common.Service;
using Whitebird.Domain.Features.Location.Entities;
using Whitebird.Domain.Features.Location.View;
using Whitebird.Infra.Features.Location;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Location.Service;

public class LocationService : BaseService, ILocationService
{
    private readonly IGenericRepository<LocationEntity> _repository;
    private readonly ILocationReps _locationReps;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public LocationService(
        IGenericRepository<LocationEntity> repository,
        ILocationReps locationReps,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<LocationService> logger) : base(logger)
    {
        _repository = repository;
        _locationReps = locationReps;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<ServiceResult<LocationDetailViewModel>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var location = await _locationReps.GetByIdWithRelationsAsync(id);
            if (location == null)
                return ServiceResult<LocationDetailViewModel>.NotFound($"Location with id {id} not found");

            var viewModel = location.Adapt<LocationDetailViewModel>();
            viewModel.ChildCount = await _locationReps.GetChildCountAsync(id);
            return ServiceResult<LocationDetailViewModel>.Success(viewModel);
        }, "get location by id");
    }

    public async Task<ServiceResult<IEnumerable<LocationListViewModel>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var locations = await _locationReps.GetAllWithRelationsAsync();
            return ServiceResult<IEnumerable<LocationListViewModel>>.Success(locations.Adapt<IEnumerable<LocationListViewModel>>());
        }, "get all locations");
    }

    public async Task<ServiceResult<IEnumerable<LocationListViewModel>>> GetActiveOnlyAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var locations = await _locationReps.GetActiveOnlyWithRelationsAsync();
            return ServiceResult<IEnumerable<LocationListViewModel>>.Success(locations.Adapt<IEnumerable<LocationListViewModel>>());
        }, "get active locations");
    }

    public async Task<ServiceResult<IEnumerable<LocationListViewModel>>> GetSubLocationsAsync(int parentId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var locations = await _locationReps.GetSubLocationAsync(parentId);
            return ServiceResult<IEnumerable<LocationListViewModel>>.Success(locations.Adapt<IEnumerable<LocationListViewModel>>());
        }, "get sub locations");
    }

    public async Task<ServiceResult<LocationDetailViewModel>> CreateAsync(LocationCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.LocationName))
            return ServiceResult<LocationDetailViewModel>.BadRequest("Location name is required");

        if (model.ParentLocationId.HasValue && model.ParentLocationId.Value > 0)
        {
            var parentExists = await _locationReps.GetByIdRawAsync(model.ParentLocationId.Value);
            if (parentExists == null)
                return ServiceResult<LocationDetailViewModel>.BadRequest($"Parent location with id {model.ParentLocationId} does not exist");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<LocationEntity>();
            entity.LocationCode = await GenerateLocationCodeAsync();
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);
            var created = await _locationReps.GetByIdWithRelationsAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    "Location",
                    created.LocationId,
                    $"Location '{created.LocationCode}' - '{created.LocationName}' created successfully",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<LocationDetailViewModel>.Failure("Failed to retrieve created location")
                : ServiceResult<LocationDetailViewModel>.Success(created.Adapt<LocationDetailViewModel>(), "Location created successfully");
        }, "create location", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Location", 0, "Create Location", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<LocationDetailViewModel>> UpdateAsync(int id, LocationUpdateViewModel model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _locationReps.GetByIdRawAsync(id);
            if (existing == null)
                return ServiceResult<LocationDetailViewModel>.NotFound($"Location with id {id} not found");

            var oldCode = existing.LocationCode;
            var oldName = existing.LocationName;
            var oldParentId = existing.ParentLocationId;

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
                return ServiceResult<LocationDetailViewModel>.Failure("Failed to update location");

            var updated = await _locationReps.GetByIdWithRelationsAsync(id);

            await _activityLogService.LogUpdateAsync(
                "Location",
                id,
                $"Location updated: Code '{oldCode}' -> '{existing.LocationCode}', Name '{oldName}' -> '{existing.LocationName}', ParentId '{oldParentId}' -> '{existing.ParentLocationId}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<LocationDetailViewModel>.Success(updated!.Adapt<LocationDetailViewModel>(), "Location updated successfully");
        }, "update location", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Location", id, "Update Location", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _locationReps.GetByIdRawAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Location with id {id} not found");

            if (await _locationReps.GetChildCountAsync(id) > 0)
                return ServiceResult.BadRequest("Cannot delete location with sub-locations");

            var result = await _repository.DeleteAsync(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(
                    "Location",
                    id,
                    $"Location '{existing.LocationCode}' - '{existing.LocationName}' deleted permanently",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to delete location")
                : ServiceResult.Success("Location deleted successfully");
        }, "delete location", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Location", id, "Delete Location", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _locationReps.GetByIdRawAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Location with id {id} not found");

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogSoftDeleteAsync(
                    "Location",
                    id,
                    $"Location '{existing.LocationCode}' - '{existing.LocationName}' soft deleted",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to soft delete location")
                : ServiceResult.Success("Location soft deleted successfully");
        }, "soft delete location", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Location", id, "Soft Delete Location", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<PaginatedResult<LocationListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var locations = await _locationReps.GetAllWithRelationsAsync();
            var query = locations.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l =>
                    l.LocationCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    l.LocationName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (l.City != null && l.City.Contains(search, StringComparison.OrdinalIgnoreCase))
                );
            }

            var totalCount = query.Count();
            var pagedData = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var viewModels = pagedData.Adapt<List<LocationListViewModel>>();

            return ServiceResult<PaginatedResult<LocationListViewModel>>.Success(new PaginatedResult<LocationListViewModel>
            {
                Data = viewModels,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }, "get location grid data");
    }

    private async Task<string> GenerateLocationCodeAsync()
    {
        var locations = await _locationReps.GetAllWithRelationsAsync();
        var maxNumber = locations
            .Where(l => l.LocationCode.StartsWith("LOC-"))
            .Select(l => int.TryParse(l.LocationCode[4..], out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"LOC-{(maxNumber + 1):D6}";
    }
}