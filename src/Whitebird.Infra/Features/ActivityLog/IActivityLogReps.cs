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
}