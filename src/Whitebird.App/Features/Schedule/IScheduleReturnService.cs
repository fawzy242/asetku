using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Schedule;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Schedule;

public interface IScheduleReturnService
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    Task<ServiceResult<ScheduleReturnView>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<ScheduleReturnListView>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<ScheduleReturnListView>>> GetByAssetIdAsync(int assetId);
    Task<ServiceResult<IEnumerable<ScheduleReturnListView>>> GetByEmployeeIdAsync(int employeeId);
    Task<ServiceResult<ScheduleReturnView>> CreateAsync(ScheduleReturnCreateDto dto);
    Task<ServiceResult<ScheduleReturnView>> UpdateAsync(int id, ScheduleReturnUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID
    // ============================================================

    Task<ServiceResult<PaginatedResult<ScheduleReturnListView>>> GetGridDataAsync(ScheduleGridRequest request);

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request);

    // ============================================================
    // GENERATE
    // ============================================================

    Task<ServiceResult<ScheduleGenerateResultDto>> GenerateAsync(ScheduleReturnGenerateDto dto);
}