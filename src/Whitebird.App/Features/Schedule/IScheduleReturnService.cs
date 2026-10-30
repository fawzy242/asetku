using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Schedule;

namespace Whitebird.App.Features.Schedule;

/// <summary>
/// Service interface for Schedule Return.
///
/// Standard CRUD / grid / bulk operations are inherited from
/// <see cref="ICommonService{TDetailView, TListView, TGridRequest, TCreateVM, TUpdateVM}"/>.
/// Only Return-specific custom operations are declared here.
/// </summary>
public interface IScheduleReturnService :
    ICommonService<ScheduleReturnView, ScheduleReturnListView, ScheduleReturnGridRequest, ScheduleReturnCreateDto, ScheduleReturnUpdateDto>
{
    Task<ServiceResult<IEnumerable<ScheduleReturnListView>>> GetByAssetIdAsync(int assetId);
    Task<ServiceResult<IEnumerable<ScheduleReturnListView>>> GetByEmployeeIdAsync(int employeeId);
    Task<ServiceResult<ScheduleReturnGenerateResultDto>> GenerateAsync(ScheduleReturnGenerateDto dto);
}