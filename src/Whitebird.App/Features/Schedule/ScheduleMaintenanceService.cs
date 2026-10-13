using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Asset;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Schedule;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Schedule;

namespace Whitebird.App.Features.Schedule;

public class ScheduleMaintenanceService : BaseService, IScheduleMaintenanceService
{
    private readonly IGenericRepository<ScheduleMaintenanceEntity> _repository;
    private readonly IScheduleMaintenanceReps _scheduleReps;
    private readonly IAssetService _assetService;
    private readonly IMasterDataService _masterDataService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public ScheduleMaintenanceService(
        IGenericRepository<ScheduleMaintenanceEntity> repository,
        IScheduleMaintenanceReps scheduleReps,
        IAssetService assetService,
        IMasterDataService masterDataService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<ScheduleMaintenanceService> logger) : base(logger)
    {
        _repository = repository;
        _scheduleReps = scheduleReps;
        _assetService = assetService;
        _masterDataService = masterDataService;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<ServiceResult<ScheduleMaintenanceView>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _scheduleReps.GetDetailByIdAsync(id);
            if (result == null)
            {
                return ServiceResult<ScheduleMaintenanceView>.NotFound($"Schedule with id {id} not found");
            }
            return ServiceResult<ScheduleMaintenanceView>.Success(result);
        }, "get schedule maintenance by id");
    }

    public async Task<ServiceResult<IEnumerable<ScheduleMaintenanceListView>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _scheduleReps.GetAllListViewAsync();
            return ServiceResult<IEnumerable<ScheduleMaintenanceListView>>.Success(result);
        }, "get all schedule maintenance");
    }

    public async Task<ServiceResult<IEnumerable<ScheduleMaintenanceListView>>> GetByAssetIdAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assetResult = await _assetService.GetByIdAsync(assetId);
            if (!assetResult.IsSuccess || assetResult.Data == null)
            {
                return ServiceResult<IEnumerable<ScheduleMaintenanceListView>>.NotFound($"Asset with id {assetId} not found");
            }

            var result = await _scheduleReps.GetByAssetIdAsync(assetId);
            return ServiceResult<IEnumerable<ScheduleMaintenanceListView>>.Success(result);
        }, "get schedule maintenance by asset");
    }

    public async Task<ServiceResult<IEnumerable<ScheduleMaintenanceListView>>> GetByMaintenanceTypeAsync(int maintenanceType)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var typeName = await _masterDataService.GetValueAsync("MaintenanceType", maintenanceType);
            if (!typeName.IsSuccess || typeName.Data == null)
            {
                return ServiceResult<IEnumerable<ScheduleMaintenanceListView>>.BadRequest($"Invalid MaintenanceType: {maintenanceType}");
            }

            var result = await _scheduleReps.GetByMaintenanceTypeAsync(maintenanceType);
            return ServiceResult<IEnumerable<ScheduleMaintenanceListView>>.Success(result);
        }, "get schedule maintenance by maintenance type");
    }

    public async Task<ServiceResult<ScheduleMaintenanceView>> CreateAsync(ScheduleMaintenanceCreateDto dto)
    {
        var assetResult = await _assetService.GetByIdAsync(dto.AssetId);
        if (!assetResult.IsSuccess || assetResult.Data == null)
        {
            return ServiceResult<ScheduleMaintenanceView>.BadRequest($"Asset with id {dto.AssetId} not found");
        }

        var typeName = await _masterDataService.GetValueAsync("MaintenanceType", dto.MaintenanceType);
        if (!typeName.IsSuccess || typeName.Data == null)
        {
            return ServiceResult<ScheduleMaintenanceView>.BadRequest($"Invalid MaintenanceType: {dto.MaintenanceType}");
        }

        var exists = await _scheduleReps.IsScheduleExistsAsync(dto.ScheduleDate, dto.AssetId);
        if (exists)
        {
            return ServiceResult<ScheduleMaintenanceView>.Conflict($"Schedule already exists for Asset {dto.AssetId} on {dto.ScheduleDate:yyyy-MM-dd}");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = dto.Adapt<ScheduleMaintenanceEntity>();
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _scheduleReps.InsertAsync(entity);
            var created = await _scheduleReps.GetDetailByIdAsync(id);

            await _activityLogService.LogCreateAsync(
                TableNames.ScheduleMaintenance,
                id,
                $"Schedule Maintenance created: Asset '{assetResult.Data.AssetCode}', Type '{typeName.Data}', Date {dto.ScheduleDate:yyyy-MM-dd}",
                _currentUserService.GetDisplayName());

            return ServiceResult<ScheduleMaintenanceView>.Success(created!, "Schedule created successfully");
        }, "create schedule maintenance");
    }

    public async Task<ServiceResult<ScheduleMaintenanceView>> UpdateAsync(int id, ScheduleMaintenanceUpdateDto dto)
    {
        var existing = await _scheduleReps.GetByIdRawAsync(id);
        if (existing == null)
        {
            return ServiceResult<ScheduleMaintenanceView>.NotFound($"Schedule with id {id} not found");
        }

        var assetResult = await _assetService.GetByIdAsync(dto.AssetId);
        if (!assetResult.IsSuccess || assetResult.Data == null)
        {
            return ServiceResult<ScheduleMaintenanceView>.BadRequest($"Asset with id {dto.AssetId} not found");
        }

        var typeName = await _masterDataService.GetValueAsync("MaintenanceType", dto.MaintenanceType);
        if (!typeName.IsSuccess || typeName.Data == null)
        {
            return ServiceResult<ScheduleMaintenanceView>.BadRequest($"Invalid MaintenanceType: {dto.MaintenanceType}");
        }

        var exists = await _scheduleReps.IsScheduleExistsAsync(dto.ScheduleDate, dto.AssetId, id);
        if (exists)
        {
            return ServiceResult<ScheduleMaintenanceView>.Conflict($"Schedule already exists for Asset {dto.AssetId} on {dto.ScheduleDate:yyyy-MM-dd}");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var wasActive = existing.IsActive;

            dto.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            // BR-01: if record was Active on entry, force Inactive after update.
            if (wasActive)
            {
                existing.IsActive = false;
            }

            await _scheduleReps.UpdateAsync(existing);
            var updated = await _scheduleReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.ScheduleMaintenance,
                id,
                $"Schedule Maintenance updated: Asset '{assetResult.Data.AssetCode}', Date {dto.ScheduleDate:yyyy-MM-dd}",
                _currentUserService.GetDisplayName());

            return ServiceResult<ScheduleMaintenanceView>.Success(updated!, "Schedule updated successfully");
        }, "update schedule maintenance");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _scheduleReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Schedule with id {id} not found");
            }

            await _scheduleReps.DeleteAsync(id);

            await _activityLogService.LogDeleteAsync(
                TableNames.ScheduleMaintenance,
                id,
                $"Schedule Maintenance deleted",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Schedule deleted successfully");
        }, "delete schedule maintenance");
    }

    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _scheduleReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Schedule with id {id} not found");
            }

            await _scheduleReps.SoftDeleteAsync(id);

            await _activityLogService.LogSoftDeleteAsync(
                TableNames.ScheduleMaintenance,
                id,
                $"Schedule Maintenance soft deleted",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Schedule soft deleted successfully");
        }, "soft delete schedule maintenance");
    }

    public async Task<ServiceResult<PaginatedResult<ScheduleMaintenanceListView>>> GetGridDataAsync(ScheduleGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _scheduleReps.GetPagedListAsync(request);
            return ServiceResult<PaginatedResult<ScheduleMaintenanceListView>>.Success(result);
        }, "get schedule maintenance grid");
    }

    public async Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No schedule IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var activatedCount = 0;

            foreach (var id in request.Ids)
            {
                var schedule = await _scheduleReps.GetByIdRawAsync(id);
                if (schedule != null && !schedule.IsActive)
                {
                    schedule.IsActive = true;
                    schedule.ModifiedDate = DateTime.Now;
                    schedule.ModifiedBy = _currentUserService.GetDisplayName();
                    await _scheduleReps.UpdateAsync(schedule);
                    activatedCount++;
                }
            }

            await _activityLogService.LogUpdateAsync(
                TableNames.ScheduleMaintenance,
                0,
                $"{activatedCount} schedule(s) activated via bulk operation",
                _currentUserService.GetDisplayName());

            return ServiceResult<int>.Success(activatedCount, $"{activatedCount} schedule(s) activated successfully");
        }, "bulk activate schedule maintenance");
    }

    public async Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No schedule IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var deactivatedCount = 0;

            foreach (var id in request.Ids)
            {
                var schedule = await _scheduleReps.GetByIdRawAsync(id);
                if (schedule != null && schedule.IsActive)
                {
                    schedule.IsActive = false;
                    schedule.ModifiedDate = DateTime.Now;
                    schedule.ModifiedBy = _currentUserService.GetDisplayName();
                    await _scheduleReps.UpdateAsync(schedule);
                    deactivatedCount++;
                }
            }

            await _activityLogService.LogUpdateAsync(
                TableNames.ScheduleMaintenance,
                0,
                $"{deactivatedCount} schedule(s) deactivated via bulk operation",
                _currentUserService.GetDisplayName());

            return ServiceResult<int>.Success(deactivatedCount, $"{deactivatedCount} schedule(s) deactivated successfully");
        }, "bulk deactivate schedule maintenance");
    }

    public async Task<ServiceResult<int>> BulkDeleteAsync(BulkDeleteRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No schedule IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var deletedCount = 0;

            foreach (var id in request.Ids)
            {
                var schedule = await _scheduleReps.GetByIdRawAsync(id);
                if (schedule == null)
                {
                    continue;
                }

                await _scheduleReps.DeleteAsync(id);
                deletedCount++;
            }

            await _activityLogService.LogDeleteAsync(
                TableNames.ScheduleMaintenance,
                0,
                $"{deletedCount} schedule(s) deleted via bulk operation",
                _currentUserService.GetDisplayName());

            return ServiceResult<int>.Success(deletedCount, $"{deletedCount} schedule(s) deleted successfully");
        }, "bulk delete schedule maintenance");
    }

    public async Task<ServiceResult<ScheduleGenerateResultDto>> GenerateAsync(ScheduleMaintenanceGenerateDto dto)
    {
        if (dto.DateFrom > dto.DateTo)
        {
            return ServiceResult<ScheduleGenerateResultDto>.BadRequest("DateFrom must be less than or equal to DateTo");
        }

        var assetResult = await _assetService.GetByIdAsync(dto.AssetId);
        if (!assetResult.IsSuccess || assetResult.Data == null)
        {
            return ServiceResult<ScheduleGenerateResultDto>.BadRequest($"Asset with id {dto.AssetId} not found");
        }

        var typeName = await _masterDataService.GetValueAsync("MaintenanceType", dto.MaintenanceType);
        if (!typeName.IsSuccess || typeName.Data == null)
        {
            return ServiceResult<ScheduleGenerateResultDto>.BadRequest($"Invalid MaintenanceType: {dto.MaintenanceType}");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var result = new ScheduleGenerateResultDto();
            var existingDates = await _scheduleReps.GetExistingDatesAsync(dto.DateFrom, dto.DateTo, dto.AssetId);
            var existingDateSet = new HashSet<DateTime>(existingDates.Select(d => d.Date));

            var currentDate = dto.DateFrom.Date;
            while (currentDate <= dto.DateTo.Date)
            {
                if (existingDateSet.Contains(currentDate))
                {
                    result.TotalSkipped++;
                    result.SkippedDates.Add(currentDate);
                }
                else
                {
                    var entity = new ScheduleMaintenanceEntity
                    {
                        ScheduleDate = currentDate,
                        AssetId = dto.AssetId,
                        MaintenanceType = dto.MaintenanceType,
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        CreatedBy = _currentUserService.GetDisplayName()
                    };

                    await _scheduleReps.InsertAsync(entity);
                    result.TotalGenerated++;
                    result.GeneratedDates.Add(currentDate);
                }

                currentDate = currentDate.AddDays(1);
            }

            await _activityLogService.LogCreateAsync(
                TableNames.ScheduleMaintenance,
                0,
                $"Generated {result.TotalGenerated} schedule(s) for Asset '{assetResult.Data.AssetCode}' from {dto.DateFrom:yyyy-MM-dd} to {dto.DateTo:yyyy-MM-dd}",
                _currentUserService.GetDisplayName());

            result.Message = $"{result.TotalGenerated} schedule(s) generated, {result.TotalSkipped} skipped (already exist)";
            return ServiceResult<ScheduleGenerateResultDto>.Success(result, result.Message);
        }, "generate schedule maintenance");
    }
}