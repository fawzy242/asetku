using Whitebird.Domain.Features.ActivityLog.View;

namespace Whitebird.Infra.Features.ActivityLog;

public interface IActivityLogReps
{
    Task<int> InsertAsync(CreateActivityLogDto log);
    Task<IEnumerable<ActivityLogListDto>> GetByReferenceAsync(string referenceTable, int referenceId);
    Task<IEnumerable<ActivityLogListDto>> GetByActivityTypeAsync(string activityType, int limit = 100);
    Task<IEnumerable<ActivityLogListDto>> GetRecentAsync(int limit = 50);
    Task<IEnumerable<ActivityLogListDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}