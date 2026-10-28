using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Schedule;

namespace Whitebird.App.Features.Schedule;

/// <summary>
/// Service interface for Schedule Maintenance.
///
/// Standard CRUD / grid / bulk operations are inherited from
/// <see cref="ICommonService{TDetailView, TListView, TGridRequest, TCreateVM, TUpdateVM}"/>.
/// Only Maintenance-specific custom operations are declared here.
/// </summary>
public interface IScheduleMaintenanceService :
    ICommonService<ScheduleMaintenanceView, ScheduleMaintenanceListView, ScheduleGridRequest, ScheduleMaintenanceCreateDto, ScheduleMaintenanceUpdateDto>
{
    Task<ServiceResult<IEnumerable<ScheduleMaintenanceListView>>> GetByAssetIdAsync(int assetId);
    Task<ServiceResult<IEnumerable<ScheduleMaintenanceListView>>> GetByMaintenanceTypeAsync(int maintenanceType);
    Task<ServiceResult<ScheduleGenerateResultDto>> GenerateAsync(ScheduleMaintenanceGenerateDto dto);
}