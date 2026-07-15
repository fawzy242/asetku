using Whitebird.Domain.Features.Schedule;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Schedule;

public interface IScheduleHandoverReps
{
    // ============================================================
    // RAW ENTITY METHODS
    // ============================================================
    
    Task<ScheduleHandoverEntity?> GetByIdRawAsync(int id);
    
    // ============================================================
    // DETAIL VIEW METHODS
    // ============================================================
    
    Task<ScheduleHandoverView?> GetDetailByIdAsync(int id);
    Task<IEnumerable<ScheduleHandoverListView>> GetAllListViewAsync();
    Task<IEnumerable<ScheduleHandoverListView>> GetByAssetIdAsync(int assetId);
    Task<IEnumerable<ScheduleHandoverListView>> GetByEmployeeIdAsync(int employeeId);
    Task<IEnumerable<ScheduleHandoverListView>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> IsScheduleExistsAsync(DateTime scheduleDate, int assetId, int? excludeId = null);
    Task<IEnumerable<DateTime>> GetExistingDatesAsync(DateTime startDate, DateTime endDate, int assetId);
    
    // ============================================================
    // CRUD
    // ============================================================
    
    Task<int> InsertAsync(ScheduleHandoverEntity entity);
    Task<int> UpdateAsync(ScheduleHandoverEntity entity);
    Task<int> DeleteAsync(int id);
    Task<int> SoftDeleteAsync(int id);
    
    // ============================================================
    // PAGINATION
    // ============================================================
    
    Task<PaginatedResult<ScheduleHandoverListView>> GetPagedListAsync(ScheduleGridRequest request);
}