using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Asset;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.Schedule;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Schedule;

namespace Whitebird.App.Features.Schedule;

/// <summary>
/// Schedule Maintenance service.
///
/// Standard CRUD / grid / bulk operations are delegated to
/// <see cref="CommonService{TEntity, TDetailView, TListView, TGridRequest, TCreateVM, TUpdateVM}"/>.
/// Maintenance-specific operations remain here.
/// </summary>
public class ScheduleMaintenanceService
    : CommonService<ScheduleMaintenanceEntity, ScheduleMaintenanceView, ScheduleMaintenanceListView, ScheduleMaintenanceGridRequest, ScheduleMaintenanceCreateDto, ScheduleMaintenanceUpdateDto>,
      IScheduleMaintenanceService
{
    private readonly IScheduleMaintenanceReps _scheduleReps;
    private readonly IAssetService _assetService;
    private readonly IMasterDataService _masterDataService;

    public ScheduleMaintenanceService(
        ICommonReps repository,
        IScheduleMaintenanceReps scheduleReps,
        IAssetService assetService,
        IMasterDataService masterDataService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<ScheduleMaintenanceService> logger)
        : base(repository, currentUserService, activityLogService, logger)
    {
        _scheduleReps = scheduleReps;
        _assetService = assetService;
        _masterDataService = masterDataService;
    }

    // ============================================================
    // HOOKS
    // ============================================================

    protected override string EntityLabel => "Schedule";
    protected override string TableName => TableNames.ScheduleMaintenance;

    protected override async Task<ScheduleMaintenanceView?> LoadDetailAsync(int id) =>
        await _scheduleReps.GetDetailByIdAsync(id);

    protected override async Task<IEnumerable<ScheduleMaintenanceListView>> LoadAllAsync() =>
        await _scheduleReps.GetAllListViewAsync();

    protected override async Task<PaginatedResult<ScheduleMaintenanceListView>> LoadPageAsync(ScheduleMaintenanceGridRequest request) =>
        await _scheduleReps.GetPagedListAsync(request);

    protected override void MapCreate(ScheduleMaintenanceCreateDto model, ScheduleMaintenanceEntity entity) => model.Adapt(entity);
    protected override void MapUpdate(ScheduleMaintenanceUpdateDto model, ScheduleMaintenanceEntity entity) => model.Adapt(entity);

    protected override string Describe(ScheduleMaintenanceEntity entity) =>
        $"Maintenance (Asset {entity.AssetId}, Type {entity.MaintenanceType}) on {entity.ScheduleDate:yyyy-MM-dd}";

    protected override async Task<ServiceResult<ScheduleMaintenanceView>?> ValidateCreateAsync(ScheduleMaintenanceCreateDto model)
    {
        var asset = await _assetService.GetByIdAsync(model.AssetId);
        if (!asset.IsSuccess || asset.Data == null)
            return ServiceResult<ScheduleMaintenanceView>.BadRequest($"Asset with id {model.AssetId} not found");

        var type = await _masterDataService.GetValueAsync("MaintenanceType", model.MaintenanceType);
        if (!type.IsSuccess || type.Data == null)
            return ServiceResult<ScheduleMaintenanceView>.BadRequest($"Invalid MaintenanceType: {model.MaintenanceType}");

        if (await _scheduleReps.IsScheduleExistsAsync(model.ScheduleDate, model.AssetId))
            return ServiceResult<ScheduleMaintenanceView>.Conflict(
                $"Schedule already exists for Asset {model.AssetId} on {model.ScheduleDate:yyyy-MM-dd}");

        return null;
    }

    protected override async Task<ServiceResult<ScheduleMaintenanceView>?> ValidateUpdateAsync(int id, ScheduleMaintenanceUpdateDto model)
    {
        var asset = await _assetService.GetByIdAsync(model.AssetId);
        if (!asset.IsSuccess || asset.Data == null)
            return ServiceResult<ScheduleMaintenanceView>.BadRequest($"Asset with id {model.AssetId} not found");

        var type = await _masterDataService.GetValueAsync("MaintenanceType", model.MaintenanceType);
        if (!type.IsSuccess || type.Data == null)
            return ServiceResult<ScheduleMaintenanceView>.BadRequest($"Invalid MaintenanceType: {model.MaintenanceType}");

        if (await _scheduleReps.IsScheduleExistsAsync(model.ScheduleDate, model.AssetId, id))
            return ServiceResult<ScheduleMaintenanceView>.Conflict(
                $"Schedule already exists for Asset {model.AssetId} on {model.ScheduleDate:yyyy-MM-dd}");

        return null;
    }

    // ============================================================
    // CUSTOM: GET BY ASSET / MAINTENANCE TYPE
    // ============================================================

    public async Task<ServiceResult<IEnumerable<ScheduleMaintenanceListView>>> GetByAssetIdAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetService.GetByIdAsync(assetId);
            if (!asset.IsSuccess || asset.Data == null)
                return ServiceResult<IEnumerable<ScheduleMaintenanceListView>>.NotFound($"Asset with id {assetId} not found");

            var result = await _scheduleReps.GetByAssetIdAsync(assetId);
            return ServiceResult<IEnumerable<ScheduleMaintenanceListView>>.Success(result);
        }, "get schedule maintenance by asset");
    }

    public async Task<ServiceResult<IEnumerable<ScheduleMaintenanceListView>>> GetByMaintenanceTypeAsync(int maintenanceType)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var type = await _masterDataService.GetValueAsync("MaintenanceType", maintenanceType);
            if (!type.IsSuccess || type.Data == null)
                return ServiceResult<IEnumerable<ScheduleMaintenanceListView>>.BadRequest($"Invalid MaintenanceType: {maintenanceType}");

            var result = await _scheduleReps.GetByMaintenanceTypeAsync(maintenanceType);
            return ServiceResult<IEnumerable<ScheduleMaintenanceListView>>.Success(result);
        }, "get schedule maintenance by maintenance type");
    }

    // ============================================================
    // CUSTOM: GENERATE
    // ============================================================

    public async Task<ServiceResult<ScheduleMaintenanceGenerateResultDto>> GenerateAsync(ScheduleMaintenanceGenerateDto dto)
    {
        if (dto.DateFrom > dto.DateTo)
            return ServiceResult<ScheduleMaintenanceGenerateResultDto>.BadRequest("DateFrom must be less than or equal to DateTo");

        var asset = await _assetService.GetByIdAsync(dto.AssetId);
        if (!asset.IsSuccess || asset.Data == null)
            return ServiceResult<ScheduleMaintenanceGenerateResultDto>.BadRequest($"Asset with id {dto.AssetId} not found");

        var type = await _masterDataService.GetValueAsync("MaintenanceType", dto.MaintenanceType);
        if (!type.IsSuccess || type.Data == null)
            return ServiceResult<ScheduleMaintenanceGenerateResultDto>.BadRequest($"Invalid MaintenanceType: {dto.MaintenanceType}");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var result = new ScheduleMaintenanceGenerateResultDto();
            var existingDates = await _scheduleReps.GetExistingDatesAsync(dto.DateFrom, dto.DateTo, dto.AssetId);
            var existingDateSet = new HashSet<DateTime>(existingDates.Select(d => d.Date));

            var current = dto.DateFrom.Date;
            while (current <= dto.DateTo.Date)
            {
                if (existingDateSet.Contains(current))
                {
                    result.TotalSkipped++;
                    result.SkippedDates.Add(current);
                }
                else
                {
                    var entity = new ScheduleMaintenanceEntity
                    {
                        ScheduleDate = current,
                        AssetId = dto.AssetId,
                        MaintenanceType = dto.MaintenanceType,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        CreatedBy = CurrentUserService.GetDisplayName()
                    };
                    await _scheduleReps.InsertAsync(entity);
                    result.TotalGenerated++;
                    result.GeneratedDates.Add(current);
                }
                current = current.AddDays(1);
            }

            await ActivityLogService.LogCreateAsync(
                TableNames.ScheduleMaintenance,
                0,
                $"Generated {result.TotalGenerated} schedule(s) for Asset '{asset.Data.AssetCode}' from {dto.DateFrom:yyyy-MM-dd} to {dto.DateTo:yyyy-MM-dd}",
                CurrentUserService.GetDisplayName());

            result.Message = $"{result.TotalGenerated} schedule(s) generated, {result.TotalSkipped} skipped (already exist)";
            return ServiceResult<ScheduleMaintenanceGenerateResultDto>.Success(result, result.Message);
        }, "generate schedule maintenance");
    }
}