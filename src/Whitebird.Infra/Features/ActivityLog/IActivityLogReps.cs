using Whitebird.Domain.Features.ActivityLog;

namespace Whitebird.Infra.Features.ActivityLog;

/// <summary>
/// Repository interface for Activity Log operations
/// </summary>
public interface IActivityLogReps
{
    /// <summary>
    /// Inserts a new activity log entry
    /// </summary>
    /// <param name="log">The activity log data to insert</param>
    /// <returns>The ID of the newly created log entry</returns>
    Task<int> InsertAsync(CreateActivityLogDto log);
    
    /// <summary>
    /// Gets all activity logs for a specific reference (table + id)
    /// </summary>
    /// <param name="referenceTable">The name of the reference table</param>
    /// <param name="referenceId">The ID of the record in the reference table</param>
    /// <returns>Collection of activity logs ordered by creation date descending</returns>
    Task<IEnumerable<ActivityLogListViewModel>> GetByReferenceAsync(string referenceTable, int referenceId);
    
    /// <summary>
    /// Gets activity logs filtered by activity type
    /// </summary>
    /// <param name="activityType">The activity type to filter by</param>
    /// <param name="limit">Maximum number of records to return (default 100)</param>
    /// <returns>Collection of activity logs ordered by creation date descending</returns>
    Task<IEnumerable<ActivityLogListViewModel>> GetByActivityTypeAsync(string activityType, int limit = 100);
    
    /// <summary>
    /// Gets the most recent activity logs
    /// </summary>
    /// <param name="limit">Maximum number of records to return (default 50)</param>
    /// <returns>Collection of recent activity logs ordered by creation date descending</returns>
    Task<IEnumerable<ActivityLogListViewModel>> GetRecentAsync(int limit = 50);
    
    /// <summary>
    /// Gets activity logs within a specific date range
    /// </summary>
    /// <param name="startDate">Start date of the range (inclusive)</param>
    /// <param name="endDate">End date of the range (inclusive)</param>
    /// <returns>Collection of activity logs ordered by creation date descending</returns>
    Task<IEnumerable<ActivityLogListViewModel>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}