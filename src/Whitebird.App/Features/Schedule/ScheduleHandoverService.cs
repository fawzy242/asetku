using Mapster;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Asset;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Employee;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.Schedule;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Schedule;

namespace Whitebird.App.Features.Schedule;

/// <summary>
/// Schedule Handover service.
///
/// Standard CRUD / grid / bulk operations are delegated to
/// <see cref="CommonService{TEntity, TDetailView, TListView, TGridRequest, TCreateVM, TUpdateVM}"/>.
/// Handover-specific operations (get by asset, get by employee, generate) remain
/// here so the custom workflow is explicit.
/// </summary>
public class ScheduleHandoverService
    : CommonService<ScheduleHandoverEntity, ScheduleHandoverView, ScheduleHandoverListView, ScheduleHandoverGridRequest, ScheduleHandoverCreateDto, ScheduleHandoverUpdateDto>,
      IScheduleHandoverService
{
    private readonly IScheduleHandoverReps _scheduleReps;
    private readonly IAssetService _assetService;
    private readonly IEmployeeService _employeeService;

    public ScheduleHandoverService(
        ICommonReps repository,
        IScheduleHandoverReps scheduleReps,
        IAssetService assetService,
        IEmployeeService employeeService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        ILogger<ScheduleHandoverService> logger)
        : base(repository, currentUserService, activityLogService, logger)
    {
        _scheduleReps = scheduleReps;
        _assetService = assetService;
        _employeeService = employeeService;
    }

    // ============================================================
    // HOOKS
    // ============================================================

    protected override string EntityLabel => "Schedule";
    protected override string TableName => TableNames.ScheduleHandover;

    protected override async Task<ScheduleHandoverView?> LoadDetailAsync(int id) =>
        await _scheduleReps.GetDetailByIdAsync(id);

    protected override async Task<IEnumerable<ScheduleHandoverListView>> LoadAllAsync() =>
        await _scheduleReps.GetAllListViewAsync();

    protected override async Task<PaginatedResult<ScheduleHandoverListView>> LoadPageAsync(ScheduleHandoverGridRequest request) =>
        await _scheduleReps.GetPagedListAsync(request);

    protected override void MapCreate(ScheduleHandoverCreateDto model, ScheduleHandoverEntity entity) => model.Adapt(entity);
    protected override void MapUpdate(ScheduleHandoverUpdateDto model, ScheduleHandoverEntity entity) => model.Adapt(entity);

    protected override string Describe(ScheduleHandoverEntity entity) =>
        $"Handover (Asset {entity.AssetId}, Employee {entity.EmployeeId}) on {entity.ScheduleDate:yyyy-MM-dd}";

    protected override async Task<ServiceResult<ScheduleHandoverView>?> ValidateCreateAsync(ScheduleHandoverCreateDto model)
    {
        var asset = await _assetService.GetByIdAsync(model.AssetId);
        if (!asset.IsSuccess || asset.Data == null)
            return ServiceResult<ScheduleHandoverView>.BadRequest($"Asset with id {model.AssetId} not found");

        var employee = await _employeeService.GetByIdAsync(model.EmployeeId);
        if (!employee.IsSuccess || employee.Data == null)
            return ServiceResult<ScheduleHandoverView>.BadRequest($"Employee with id {model.EmployeeId} not found");

        if (await _scheduleReps.IsScheduleExistsAsync(model.ScheduleDate, model.AssetId))
            return ServiceResult<ScheduleHandoverView>.Conflict(
                $"Schedule already exists for Asset {model.AssetId} on {model.ScheduleDate:yyyy-MM-dd}");

        return null;
    }

    protected override async Task<ServiceResult<ScheduleHandoverView>?> ValidateUpdateAsync(int id, ScheduleHandoverUpdateDto model)
    {
        var asset = await _assetService.GetByIdAsync(model.AssetId);
        if (!asset.IsSuccess || asset.Data == null)
            return ServiceResult<ScheduleHandoverView>.BadRequest($"Asset with id {model.AssetId} not found");

        var employee = await _employeeService.GetByIdAsync(model.EmployeeId);
        if (!employee.IsSuccess || employee.Data == null)
            return ServiceResult<ScheduleHandoverView>.BadRequest($"Employee with id {model.EmployeeId} not found");

        if (await _scheduleReps.IsScheduleExistsAsync(model.ScheduleDate, model.AssetId, id))
            return ServiceResult<ScheduleHandoverView>.Conflict(
                $"Schedule already exists for Asset {model.AssetId} on {model.ScheduleDate:yyyy-MM-dd}");

        return null;
    }

    // ============================================================
    // CUSTOM: GET BY ASSET / EMPLOYEE
    // ============================================================

    public async Task<ServiceResult<IEnumerable<ScheduleHandoverListView>>> GetByAssetIdAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetService.GetByIdAsync(assetId);
            if (!asset.IsSuccess || asset.Data == null)
                return ServiceResult<IEnumerable<ScheduleHandoverListView>>.NotFound($"Asset with id {assetId} not found");

            var result = await _scheduleReps.GetByAssetIdAsync(assetId);
            return ServiceResult<IEnumerable<ScheduleHandoverListView>>.Success(result);
        }, "get schedule handover by asset");
    }

    public async Task<ServiceResult<IEnumerable<ScheduleHandoverListView>>> GetByEmployeeIdAsync(int employeeId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var employee = await _employeeService.GetByIdAsync(employeeId);
            if (!employee.IsSuccess || employee.Data == null)
                return ServiceResult<IEnumerable<ScheduleHandoverListView>>.NotFound($"Employee with id {employeeId} not found");

            var result = await _scheduleReps.GetByEmployeeIdAsync(employeeId);
            return ServiceResult<IEnumerable<ScheduleHandoverListView>>.Success(result);
        }, "get schedule handover by employee");
    }

    // ============================================================
    // CUSTOM: GENERATE
    // ============================================================

    public async Task<ServiceResult<ScheduleHandoverGenerateResultDto>> GenerateAsync(ScheduleHandoverGenerateDto dto)
    {
        if (dto.DateFrom > dto.DateTo)
            return ServiceResult<ScheduleHandoverGenerateResultDto>.BadRequest("DateFrom must be less than or equal to DateTo");

        var asset = await _assetService.GetByIdAsync(dto.AssetId);
        if (!asset.IsSuccess || asset.Data == null)
            return ServiceResult<ScheduleHandoverGenerateResultDto>.BadRequest($"Asset with id {dto.AssetId} not found");

        var employee = await _employeeService.GetByIdAsync(dto.EmployeeId);
        if (!employee.IsSuccess || employee.Data == null)
            return ServiceResult<ScheduleHandoverGenerateResultDto>.BadRequest($"Employee with id {dto.EmployeeId} not found");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var result = new ScheduleHandoverGenerateResultDto();
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
                    var entity = new ScheduleHandoverEntity
                    {
                        ScheduleDate = current,
                        AssetId = dto.AssetId,
                        EmployeeId = dto.EmployeeId,
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
                TableNames.ScheduleHandover,
                0,
                $"Generated {result.TotalGenerated} schedule(s) for Asset '{asset.Data.AssetCode}' from {dto.DateFrom:yyyy-MM-dd} to {dto.DateTo:yyyy-MM-dd}",
                CurrentUserService.GetDisplayName());

            result.Message = $"{result.TotalGenerated} schedule(s) generated, {result.TotalSkipped} skipped (already exist)";
            return ServiceResult<ScheduleHandoverGenerateResultDto>.Success(result, result.Message);
        }, "generate schedule handover");
    }
}