using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Office;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Office;

namespace Whitebird.App.Features.Office;

public class OfficeService
    : CommonService<OfficeEntity, OfficeDetailView, OfficeListView, OfficeGridRequest, OfficeCreateViewModel, OfficeUpdateViewModel>,
      IOfficeService
{
    private readonly IOfficeReps _officeReps;
    private readonly IMasterDataService _masterDataService;

    public OfficeService(
        ICommonReps repository,
        IOfficeReps officeReps,
        IMasterDataService masterDataService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<OfficeService> logger)
        : base(repository, currentUserService, activityLogService, logger)
    {
        _officeReps = officeReps;
        _masterDataService = masterDataService;
    }

    protected override string EntityLabel => "Office";
    protected override string TableName => TableNames.Office;

    protected override string BulkDeactivateEmptyMessage =>
        "No offices were deactivated. Offices with child offices cannot be deactivated.";
    protected override string BulkDeleteEmptyMessage =>
        "No offices were deleted. Offices with child offices, employees, or transactions cannot be deleted.";

    protected override async Task<OfficeDetailView?> LoadDetailAsync(int id) =>
        await _officeReps.GetDetailByIdAsync(id);

    protected override async Task<IEnumerable<OfficeListView>> LoadAllAsync() =>
        await _officeReps.GetAllListViewAsync();

    protected override async Task<PaginatedResult<OfficeListView>> LoadPageAsync(OfficeGridRequest request) =>
        await _officeReps.GetPagedListAsync(request);

    protected override async Task<ServiceResult<OfficeDetailView>?> ValidateCreateAsync(OfficeCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.OfficeName))
            return ServiceResult<OfficeDetailView>.BadRequest("Office name is required");

        if (await _officeReps.IsOfficeNameExistsAsync(model.OfficeName))
            return ServiceResult<OfficeDetailView>.Conflict($"Office '{model.OfficeName}' already exists");

        if (!string.IsNullOrWhiteSpace(model.OfficeCode) &&
            await _officeReps.IsOfficeCodeExistsAsync(model.OfficeCode))
            return ServiceResult<OfficeDetailView>.Conflict($"Office code '{model.OfficeCode}' already exists");

        if (model.ParentOfficeId.HasValue && model.ParentOfficeId.Value > 0)
        {
            if (await _officeReps.GetByIdRawAsync(model.ParentOfficeId.Value) == null)
                return ServiceResult<OfficeDetailView>.BadRequest(
                    $"Parent office with id {model.ParentOfficeId} does not exist");
        }

        if (model.OfficeType.HasValue)
        {
            var typeExists = await _masterDataService.GetValueAsync("OfficeType", model.OfficeType.Value);
            if (!typeExists.IsSuccess || typeExists.Data == null)
                return ServiceResult<OfficeDetailView>.BadRequest($"Invalid office type: {model.OfficeType}");
        }

        return null;
    }

    protected override async Task<ServiceResult<OfficeDetailView>?> ValidateUpdateAsync(int id, OfficeUpdateViewModel model)
    {
        if (await _officeReps.IsOfficeNameExistsAsync(model.OfficeName, id))
            return ServiceResult<OfficeDetailView>.Conflict($"Office '{model.OfficeName}' already exists");

        if (!string.IsNullOrWhiteSpace(model.OfficeCode) &&
            await _officeReps.IsOfficeCodeExistsAsync(model.OfficeCode, id))
            return ServiceResult<OfficeDetailView>.Conflict($"Office code '{model.OfficeCode}' already exists");

        if (model.ParentOfficeId.HasValue && model.ParentOfficeId.Value > 0)
        {
            if (model.ParentOfficeId.Value == id)
                return ServiceResult<OfficeDetailView>.BadRequest("Office cannot be parent of itself");

            if (await _officeReps.GetByIdRawAsync(model.ParentOfficeId.Value) == null)
                return ServiceResult<OfficeDetailView>.BadRequest(
                    $"Parent office with id {model.ParentOfficeId} does not exist");

            var allOffices = await _officeReps.GetAllListViewAsync();
            if (allOffices.Any(c => c.OfficeId == model.ParentOfficeId.Value && c.ParentOfficeId == id))
                return ServiceResult<OfficeDetailView>.BadRequest("Circular reference detected: child cannot become parent");
        }

        if (model.OfficeType.HasValue)
        {
            var typeExists = await _masterDataService.GetValueAsync("OfficeType", model.OfficeType.Value);
            if (!typeExists.IsSuccess || typeExists.Data == null)
                return ServiceResult<OfficeDetailView>.BadRequest($"Invalid office type: {model.OfficeType}");
        }

        return null;
    }

    protected override void MapCreate(OfficeCreateViewModel model, OfficeEntity entity) => model.Adapt(entity);
    protected override void MapUpdate(OfficeUpdateViewModel model, OfficeEntity entity) => model.Adapt(entity);
    protected override string Describe(OfficeEntity entity) => entity.OfficeName;

    protected override async Task<(bool CanProceed, string? Reason)> CanDeleteAsync(int id)
    {
        var childCount = await _officeReps.GetChildCountAsync(id);
        if (childCount > 0)
            return (false, $"Cannot delete office with {childCount} sub-offices");

        var employeeCount = await _officeReps.GetEmployeeCountAsync(id);
        if (employeeCount > 0)
            return (false, $"Cannot delete office with {employeeCount} associated employee(s)");

        var transactionCount = await _officeReps.GetAssetTransactionCountAsync(id);
        if (transactionCount > 0)
            return (false, $"Cannot delete office with {transactionCount} associated transaction(s)");

        return (true, (string?)null);
    }

    protected override async Task<(bool CanProceed, string? Reason)> CanDeactivateAsync(int id)
    {
        var childCount = await _officeReps.GetChildCountAsync(id);
        return childCount > 0 ? (false, (string?)null) : (true, (string?)null);
    }

    public async Task<ServiceResult<IEnumerable<OfficeDropdownView>>> GetDropdownListAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var offices = await _officeReps.GetDropdownListAsync();
            return ServiceResult<IEnumerable<OfficeDropdownView>>.Success(offices);
        }, "get office dropdown list");
    }
}