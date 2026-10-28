using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Schedule;

namespace Whitebird.App.Features.Schedule;

/// <summary>
/// Service interface for Schedule Handover.
///
/// Standard CRUD / grid / bulk operations are inherited from
/// <see cref="ICommonService{TDetailView, TListView, TGridRequest, TCreateVM, TUpdateVM}"/>.
/// Only Handover-specific custom operations are declared here.
/// </summary>
public interface IScheduleHandoverService :
    ICommonService<ScheduleHandoverView, ScheduleHandoverListView, ScheduleGridRequest, ScheduleHandoverCreateDto, ScheduleHandoverUpdateDto>
{
    Task<ServiceResult<IEnumerable<ScheduleHandoverListView>>> GetByAssetIdAsync(int assetId);
    Task<ServiceResult<IEnumerable<ScheduleHandoverListView>>> GetByEmployeeIdAsync(int employeeId);
    Task<ServiceResult<ScheduleGenerateResultDto>> GenerateAsync(ScheduleHandoverGenerateDto dto);
}