using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Schedule;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Schedule;

public interface IScheduleMaintenanceService
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    Task<ServiceResult<ScheduleMaintenanceView>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<ScheduleMaintenanceListView>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<ScheduleMaintenanceListView>>> GetByAssetIdAsync(int assetId);
    Task<ServiceResult<IEnumerable<ScheduleMaintenanceListView>>> GetByMaintenanceTypeAsync(int maintenanceType);
    Task<ServiceResult<ScheduleMaintenanceView>> CreateAsync(ScheduleMaintenanceCreateDto dto);
    Task<ServiceResult<ScheduleMaintenanceView>> UpdateAsync(int id, ScheduleMaintenanceUpdateDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID - Using GridRequest
    // ============================================================

    Task<ServiceResult<PaginatedResult<ScheduleMaintenanceListView>>> GetGridDataAsync(ScheduleGridRequest request);

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeleteAsync(BulkDeleteRequest request);

    // ============================================================
    // GENERATE
    // ============================================================

    Task<ServiceResult<ScheduleGenerateResultDto>> GenerateAsync(ScheduleMaintenanceGenerateDto dto);
}