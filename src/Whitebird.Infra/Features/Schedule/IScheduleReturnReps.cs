using Whitebird.Domain.Features.Schedule;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Schedule;

public interface IScheduleReturnReps
{
    // ============================================================
    // RAW ENTITY METHODS
    // ============================================================

    Task<ScheduleReturnEntity?> GetByIdRawAsync(int id);

    // ============================================================
    // DETAIL VIEW METHODS
    // ============================================================

    Task<ScheduleReturnView?> GetDetailByIdAsync(int id);
    Task<IEnumerable<ScheduleReturnListView>> GetAllListViewAsync();
    Task<IEnumerable<ScheduleReturnListView>> GetByAssetIdAsync(int assetId);
    Task<IEnumerable<ScheduleReturnListView>> GetByEmployeeIdAsync(int employeeId);
    Task<bool> IsScheduleExistsAsync(DateTime scheduleDate, int assetId, int? excludeId = null);
    Task<IEnumerable<DateTime>> GetExistingDatesAsync(DateTime startDate, DateTime endDate, int assetId);

    // ============================================================
    // CRUD
    // ============================================================

    Task<int> InsertAsync(ScheduleReturnEntity entity);
    Task<int> UpdateAsync(ScheduleReturnEntity entity);
    Task<int> DeleteAsync(int id);
    Task<int> SoftDeleteAsync(int id);

    // ============================================================
    // PAGINATION
    // ============================================================

    Task<PaginatedResult<ScheduleReturnListView>> GetPagedListAsync(ScheduleReturnGridRequest request);
}