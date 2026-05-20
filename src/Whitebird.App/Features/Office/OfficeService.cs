using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Office;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Office;

namespace Whitebird.App.Features.Office;

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

    public async Task<ServiceResult<OfficeDetailViewModel>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var office = await _officeReps.GetByIdWithRelationsAsync(id);
            if (office == null)
                return ServiceResult<OfficeDetailViewModel>.NotFound($"Office with id {id} not found");

            var viewModel = office.Adapt<OfficeDetailViewModel>();
            viewModel.ChildCount = await _officeReps.GetChildCountAsync(id);
            
            if (viewModel.OfficeType.HasValue)
            {
                var typeResult = await _masterDataService.GetValueAsync("OfficeType", viewModel.OfficeType.Value);
                if (typeResult.IsSuccess)
                    viewModel.OfficeTypeName = typeResult.Data;
            }

            return ServiceResult<OfficeDetailViewModel>.Success(viewModel);
        }, "get office by id");
    }

    public async Task<ServiceResult<IEnumerable<OfficeListViewModel>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var offices = await _officeReps.GetAllAsync();
            var viewModels = offices.Adapt<List<OfficeListViewModel>>();
            
            var officeTypes = await _masterDataService.GetOfficeTypesAsync();
            var typeDict = officeTypes.IsSuccess && officeTypes.Data != null
                ? officeTypes.Data.ToDictionary(t => t.Code, t => t.Name)
                : new Dictionary<int, string>();

            foreach (var vm in viewModels)
            {
                if (vm.OfficeType.HasValue && typeDict.ContainsKey(vm.OfficeType.Value))
                    vm.OfficeTypeName = typeDict[vm.OfficeType.Value];
            }

            return ServiceResult<IEnumerable<OfficeListViewModel>>.Success(viewModels);
        }, "get all offices");
    }

    public async Task<ServiceResult<IEnumerable<OfficeListViewModel>>> GetActiveOnlyAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var offices = await _officeReps.GetActiveOnlyAsync();
            var viewModels = offices.Adapt<List<OfficeListViewModel>>();
            
            var officeTypes = await _masterDataService.GetOfficeTypesAsync();
            var typeDict = officeTypes.IsSuccess && officeTypes.Data != null
                ? officeTypes.Data.ToDictionary(t => t.Code, t => t.Name)
                : new Dictionary<int, string>();

            foreach (var vm in viewModels)
            {
                if (vm.OfficeType.HasValue && typeDict.ContainsKey(vm.OfficeType.Value))
                    vm.OfficeTypeName = typeDict[vm.OfficeType.Value];
            }

            return ServiceResult<IEnumerable<OfficeListViewModel>>.Success(viewModels);
        }, "get active offices");
    }

    public async Task<ServiceResult<IEnumerable<OfficeListViewModel>>> GetSubOfficesAsync(int parentId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var offices = await _officeReps.GetSubOfficesAsync(parentId);
            var viewModels = offices.Adapt<List<OfficeListViewModel>>();
            return ServiceResult<IEnumerable<OfficeListViewModel>>.Success(viewModels);
        }, "get sub offices");
    }

    public async Task<ServiceResult<OfficeDetailViewModel>> CreateAsync(OfficeCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.OfficeName))
            return ServiceResult<OfficeDetailViewModel>.BadRequest("Office name is required");

        if (await _officeReps.IsOfficeNameExistsAsync(model.OfficeName))
            return ServiceResult<OfficeDetailViewModel>.Conflict($"Office '{model.OfficeName}' already exists");

        if (!string.IsNullOrWhiteSpace(model.OfficeCode) && 
            await _officeReps.IsOfficeCodeExistsAsync(model.OfficeCode))
            return ServiceResult<OfficeDetailViewModel>.Conflict($"Office code '{model.OfficeCode}' already exists");

        if (model.ParentOfficeId.HasValue && model.ParentOfficeId.Value > 0)
        {
            var parentExists = await _officeReps.GetByIdAsync(model.ParentOfficeId.Value);
            if (parentExists == null)
                return ServiceResult<OfficeDetailViewModel>.BadRequest($"Parent office with id {model.ParentOfficeId} does not exist");
        }

        if (model.OfficeType.HasValue)
        {
            var typeExists = await _masterDataService.GetValueAsync("OfficeType", model.OfficeType.Value);
            if (!typeExists.IsSuccess || typeExists.Data == null)
                return ServiceResult<OfficeDetailViewModel>.BadRequest($"Invalid office type: {model.OfficeType}");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<OfficeEntity>();
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);
            var created = await _officeReps.GetByIdWithRelationsAsync(Convert.ToInt32(id));

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    "Office",
                    created.OfficeId,
                    $"Office '{created.OfficeName}' created successfully",
                    _currentUserService.GetDisplayName());
            }

            if (created == null)
                return ServiceResult<OfficeDetailViewModel>.Failure("Failed to retrieve created office");

            var viewModel = created.Adapt<OfficeDetailViewModel>();
            if (viewModel.OfficeType.HasValue)
            {
                var typeResult = await _masterDataService.GetValueAsync("OfficeType", viewModel.OfficeType.Value);
                if (typeResult.IsSuccess)
                    viewModel.OfficeTypeName = typeResult.Data;
            }

            return ServiceResult<OfficeDetailViewModel>.Success(viewModel, "Office created successfully");
        }, "create office", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Office", 0, "Create Office", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<OfficeDetailViewModel>> UpdateAsync(int id, OfficeUpdateViewModel model)
    {
        if (await _officeReps.IsOfficeNameExistsAsync(model.OfficeName, id))
            return ServiceResult<OfficeDetailViewModel>.Conflict($"Office '{model.OfficeName}' already exists");

        if (!string.IsNullOrWhiteSpace(model.OfficeCode) && 
            await _officeReps.IsOfficeCodeExistsAsync(model.OfficeCode, id))
            return ServiceResult<OfficeDetailViewModel>.Conflict($"Office code '{model.OfficeCode}' already exists");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _officeReps.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult<OfficeDetailViewModel>.NotFound($"Office with id {id} not found");

            if (model.ParentOfficeId.HasValue && model.ParentOfficeId.Value > 0)
            {
                if (model.ParentOfficeId.Value == id)
                    return ServiceResult<OfficeDetailViewModel>.BadRequest("Office cannot be parent of itself");

                var parent = await _officeReps.GetByIdAsync(model.ParentOfficeId.Value);
                if (parent == null)
                    return ServiceResult<OfficeDetailViewModel>.BadRequest($"Parent office with id {model.ParentOfficeId} does not exist");

                var children = await _officeReps.GetSubOfficesAsync(id);
                if (children.Any(c => c.OfficeId == model.ParentOfficeId.Value))
                    return ServiceResult<OfficeDetailViewModel>.BadRequest("Circular reference detected: child cannot become parent");
            }

            var oldName = existing.OfficeName;
            var oldCode = existing.OfficeCode;
            var oldParentId = existing.ParentOfficeId;

            model.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
                return ServiceResult<OfficeDetailViewModel>.Failure("Failed to update office");

            var updated = await _officeReps.GetByIdWithRelationsAsync(id);

            await _activityLogService.LogUpdateAsync(
                "Office",
                id,
                $"Office updated: Name '{oldName}' -> '{model.OfficeName}', Code '{oldCode}' -> '{model.OfficeCode}', ParentId '{oldParentId}' -> '{model.ParentOfficeId}'",
                _currentUserService.GetDisplayName());

            var viewModel = updated!.Adapt<OfficeDetailViewModel>();
            if (viewModel.OfficeType.HasValue)
            {
                var typeResult = await _masterDataService.GetValueAsync("OfficeType", viewModel.OfficeType.Value);
                if (typeResult.IsSuccess)
                    viewModel.OfficeTypeName = typeResult.Data;
            }

            return ServiceResult<OfficeDetailViewModel>.Success(viewModel, "Office updated successfully");
        }, "update office", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Office", id, "Update Office", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _officeReps.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Office with id {id} not found");

            if (await _officeReps.GetChildCountAsync(id) > 0)
                return ServiceResult.BadRequest("Cannot delete office with sub-offices");

            var result = await _repository.DeleteAsync(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(
                    "Office",
                    id,
                    $"Office '{existing.OfficeName}' deleted permanently",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to delete office")
                : ServiceResult.Success("Office deleted successfully");
        }, "delete office", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Office", id, "Delete Office", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _officeReps.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult.NotFound($"Office with id {id} not found");

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogSoftDeleteAsync(
                    "Office",
                    id,
                    $"Office '{existing.OfficeName}' soft deleted",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to soft delete office")
                : ServiceResult.Success("Office soft deleted successfully");
        }, "soft delete office", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("Office", id, "Soft Delete Office", ex, _currentUserService.GetDisplayName());
        });
    }

    public async Task<ServiceResult<PaginatedResult<OfficeListViewModel>>> GetGridDataAsync(int page, int pageSize, string? search = null)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var offices = await _officeReps.GetAllAsync();
            var query = offices.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(o =>
                    (o.OfficeCode != null && o.OfficeCode.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    o.OfficeName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (o.City != null && o.City.Contains(search, StringComparison.OrdinalIgnoreCase))
                );
            }

            var totalCount = query.Count();
            var pagedData = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var viewModels = pagedData.Adapt<List<OfficeListViewModel>>();

            var officeTypes = await _masterDataService.GetOfficeTypesAsync();
            var typeDict = officeTypes.IsSuccess && officeTypes.Data != null
                ? officeTypes.Data.ToDictionary(t => t.Code, t => t.Name)
                : new Dictionary<int, string>();

            foreach (var vm in viewModels)
            {
                if (vm.OfficeType.HasValue && typeDict.ContainsKey(vm.OfficeType.Value))
                    vm.OfficeTypeName = typeDict[vm.OfficeType.Value];
            }

            return ServiceResult<PaginatedResult<OfficeListViewModel>>.Success(new PaginatedResult<OfficeListViewModel>
            {
                Data = viewModels,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }, "get office grid data");
    }
}