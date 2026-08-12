using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Schedule;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Schedule;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.Employee;

namespace Whitebird.App.Features.Schedule;

public class ScheduleReturnService : BaseService, IScheduleReturnService
{
    private readonly IGenericRepository<ScheduleReturnEntity> _repository;
    private readonly IScheduleReturnReps _scheduleReps;
    private readonly IAssetReps _assetReps;
    private readonly IEmployeeReps _employeeReps;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;

    public ScheduleReturnService(
        IGenericRepository<ScheduleReturnEntity> repository,
        IScheduleReturnReps scheduleReps,
        IAssetReps assetReps,
        IEmployeeReps employeeReps,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<ScheduleReturnService> logger) : base(logger)
    {
        _repository = repository;
        _scheduleReps = scheduleReps;
        _assetReps = assetReps;
        _employeeReps = employeeReps;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
    }

    public async Task<ServiceResult<ScheduleReturnView>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _scheduleReps.GetDetailByIdAsync(id);
            if (result == null)
            {
                return ServiceResult<ScheduleReturnView>.NotFound($"Schedule with id {id} not found");
            }
            return ServiceResult<ScheduleReturnView>.Success(result);
        }, "get schedule return by id");
    }

    public async Task<ServiceResult<IEnumerable<ScheduleReturnListView>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _scheduleReps.GetAllListViewAsync();
            return ServiceResult<IEnumerable<ScheduleReturnListView>>.Success(result);
        }, "get all schedule return");
    }

    public async Task<ServiceResult<IEnumerable<ScheduleReturnListView>>> GetByAssetIdAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetByIdRawAsync(assetId);
            if (asset == null)
            {
                return ServiceResult<IEnumerable<ScheduleReturnListView>>.NotFound($"Asset with id {assetId} not found");
            }

            var result = await _scheduleReps.GetByAssetIdAsync(assetId);
            return ServiceResult<IEnumerable<ScheduleReturnListView>>.Success(result);
        }, "get schedule return by asset");
    }

    public async Task<ServiceResult<IEnumerable<ScheduleReturnListView>>> GetByEmployeeIdAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employee = await _employeeReps.GetByIdRawAsync(employeeId);
            if (employee == null)
            {
                return ServiceResult<IEnumerable<ScheduleReturnListView>>.NotFound($"Employee with id {employeeId} not found");
            }

            var result = await _scheduleReps.GetByEmployeeIdAsync(employeeId);
            return ServiceResult<IEnumerable<ScheduleReturnListView>>.Success(result);
        }, "get schedule return by employee");
    }

    public async Task<ServiceResult<ScheduleReturnView>> CreateAsync(ScheduleReturnCreateDto dto)
    {
        var asset = await _assetReps.GetByIdRawAsync(dto.AssetId);
        if (asset == null)
        {
            return ServiceResult<ScheduleReturnView>.BadRequest($"Asset with id {dto.AssetId} not found");
        }

        var employee = await _employeeReps.GetByIdRawAsync(dto.EmployeeId);
        if (employee == null)
        {
            return ServiceResult<ScheduleReturnView>.BadRequest($"Employee with id {dto.EmployeeId} not found");
        }

        var exists = await _scheduleReps.IsScheduleExistsAsync(dto.ScheduleDate, dto.AssetId);
        if (exists)
        {
            return ServiceResult<ScheduleReturnView>.Conflict($"Schedule already exists for Asset {dto.AssetId} on {dto.ScheduleDate:yyyy-MM-dd}");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = dto.Adapt<ScheduleReturnEntity>();
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _scheduleReps.InsertAsync(entity);
            var created = await _scheduleReps.GetDetailByIdAsync(id);

            await _activityLogService.LogCreateAsync(
                TableNames.ScheduleReturn,
                id,
                $"Schedule Return created: Asset '{asset.AssetCode}', Employee '{employee.FullName}', Date {dto.ScheduleDate:yyyy-MM-dd}",
                _currentUserService.GetDisplayName());

            return ServiceResult<ScheduleReturnView>.Success(created!, "Schedule created successfully");
        }, "create schedule return");
    }

    public async Task<ServiceResult<ScheduleReturnView>> UpdateAsync(int id, ScheduleReturnUpdateDto dto)
    {
        var existing = await _scheduleReps.GetByIdRawAsync(id);
        if (existing == null)
        {
            return ServiceResult<ScheduleReturnView>.NotFound($"Schedule with id {id} not found");
        }

        var asset = await _assetReps.GetByIdRawAsync(dto.AssetId);
        if (asset == null)
        {
            return ServiceResult<ScheduleReturnView>.BadRequest($"Asset with id {dto.AssetId} not found");
        }

        var employee = await _employeeReps.GetByIdRawAsync(dto.EmployeeId);
        if (employee == null)
        {
            return ServiceResult<ScheduleReturnView>.BadRequest($"Employee with id {dto.EmployeeId} not found");
        }

        var exists = await _scheduleReps.IsScheduleExistsAsync(dto.ScheduleDate, dto.AssetId, id);
        if (exists)
        {
            return ServiceResult<ScheduleReturnView>.Conflict($"Schedule already exists for Asset {dto.AssetId} on {dto.ScheduleDate:yyyy-MM-dd}");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            dto.Adapt(existing);
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            await _scheduleReps.UpdateAsync(existing);
            var updated = await _scheduleReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(
                TableNames.ScheduleReturn,
                id,
                $"Schedule Return updated: Asset '{asset.AssetCode}', Date {dto.ScheduleDate:yyyy-MM-dd}",
                _currentUserService.GetDisplayName());

            return ServiceResult<ScheduleReturnView>.Success(updated!, "Schedule updated successfully");
        }, "update schedule return");
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
                TableNames.ScheduleReturn,
                id,
                $"Schedule Return deleted",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Schedule deleted successfully");
        }, "delete schedule return");
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
                TableNames.ScheduleReturn,
                id,
                $"Schedule Return soft deleted",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Schedule soft deleted successfully");
        }, "soft delete schedule return");
    }

    public async Task<ServiceResult<PaginatedResult<ScheduleReturnListView>>> GetGridDataAsync(ScheduleGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _scheduleReps.GetPagedListAsync(request);
            return ServiceResult<PaginatedResult<ScheduleReturnListView>>.Success(result);
        }, "get schedule return grid");
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
                TableNames.ScheduleReturn,
                0,
                $"{activatedCount} schedule(s) activated via bulk operation",
                _currentUserService.GetDisplayName());

            return ServiceResult<int>.Success(activatedCount, $"{activatedCount} schedule(s) activated successfully");
        }, "bulk activate schedule return");
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
                TableNames.ScheduleReturn,
                0,
                $"{deactivatedCount} schedule(s) deactivated via bulk operation",
                _currentUserService.GetDisplayName());

            return ServiceResult<int>.Success(deactivatedCount, $"{deactivatedCount} schedule(s) deactivated successfully");
        }, "bulk deactivate schedule return");
    }

    public async Task<ServiceResult<ScheduleGenerateResultDto>> GenerateAsync(ScheduleReturnGenerateDto dto)
    {
        if (dto.DateFrom > dto.DateTo)
        {
            return ServiceResult<ScheduleGenerateResultDto>.BadRequest("DateFrom must be less than or equal to DateTo");
        }

        var asset = await _assetReps.GetByIdRawAsync(dto.AssetId);
        if (asset == null)
        {
            return ServiceResult<ScheduleGenerateResultDto>.BadRequest($"Asset with id {dto.AssetId} not found");
        }

        var employee = await _employeeReps.GetByIdRawAsync(dto.EmployeeId);
        if (employee == null)
        {
            return ServiceResult<ScheduleGenerateResultDto>.BadRequest($"Employee with id {dto.EmployeeId} not found");
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
                    var entity = new ScheduleReturnEntity
                    {
                        ScheduleDate = currentDate,
                        AssetId = dto.AssetId,
                        EmployeeId = dto.EmployeeId,
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
                TableNames.ScheduleReturn,
                0,
                $"Generated {result.TotalGenerated} schedule(s) for Asset '{asset.AssetCode}' from {dto.DateFrom:yyyy-MM-dd} to {dto.DateTo:yyyy-MM-dd}",
                _currentUserService.GetDisplayName());

            result.Message = $"{result.TotalGenerated} schedule(s) generated, {result.TotalSkipped} skipped (already exist)";
            return ServiceResult<ScheduleGenerateResultDto>.Success(result, result.Message);
        }, "generate schedule return");
    }
}