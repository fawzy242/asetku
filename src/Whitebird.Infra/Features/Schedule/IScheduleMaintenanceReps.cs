using Whitebird.Domain.Features.Schedule;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Schedule;

public interface IScheduleMaintenanceReps
{
    // ============================================================
    // RAW ENTITY METHODS
    // ============================================================
    
    Task<ScheduleMaintenanceEntity?> GetByIdRawAsync(int id);
    
    // ============================================================
    // DETAIL VIEW METHODS
    // ============================================================
    
    Task<ScheduleMaintenanceView?> GetDetailByIdAsync(int id);
    Task<IEnumerable<ScheduleMaintenanceListView>> GetAllListViewAsync();
    Task<IEnumerable<ScheduleMaintenanceListView>> GetByAssetIdAsync(int assetId);
    Task<IEnumerable<ScheduleMaintenanceListView>> GetByMaintenanceTypeAsync(int maintenanceType);
    Task<IEnumerable<ScheduleMaintenanceListView>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<bool> IsScheduleExistsAsync(DateTime scheduleDate, int assetId, int? excludeId = null);
    Task<IEnumerable<DateTime>> GetExistingDatesAsync(DateTime startDate, DateTime endDate, int assetId);
    
    // ============================================================
    // CRUD
    // ============================================================
    
    Task<int> InsertAsync(ScheduleMaintenanceEntity entity);
    Task<int> UpdateAsync(ScheduleMaintenanceEntity entity);
    Task<int> DeleteAsync(int id);
    Task<int> SoftDeleteAsync(int id);
    
    // ============================================================
    // PAGINATION
    // ============================================================
    
    Task<PaginatedResult<ScheduleMaintenanceListView>> GetPagedListAsync(ScheduleGridRequest request);
}