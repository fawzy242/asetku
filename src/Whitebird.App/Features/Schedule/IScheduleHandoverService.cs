using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Schedule;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Schedule;

public interface IScheduleHandoverService
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    Task<ServiceResult<ScheduleHandoverView>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<ScheduleHandoverListView>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<ScheduleHandoverListView>>> GetByAssetIdAsync(int assetId);
    Task<ServiceResult<IEnumerable<ScheduleHandoverListView>>> GetByEmployeeIdAsync(int employeeId);
    Task<ServiceResult<ScheduleHandoverView>> CreateAsync(ScheduleHandoverCreateDto dto);
    Task<ServiceResult<ScheduleHandoverView>> UpdateAsync(int id, ScheduleHandoverUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID
    // ============================================================

    Task<ServiceResult<PaginatedResult<ScheduleHandoverListView>>> GetGridDataAsync(ScheduleGridRequest request);

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request);

    // ============================================================
    // GENERATE
    // ============================================================

    Task<ServiceResult<ScheduleGenerateResultDto>> GenerateAsync(ScheduleHandoverGenerateDto dto);
}