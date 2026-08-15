using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.ActivityLog;

namespace Whitebird.Infra.Features.ActivityLog;

/// <summary>
/// Repository implementation for Activity Log operations using Dapper
/// </summary>
public class ActivityLogReps : IActivityLogReps
{
    private readonly DapperContext _context;

    public ActivityLogReps(DapperContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<int> InsertAsync(CreateActivityLogDto log)
    {
        const string sql = @"
            INSERT INTO ActivityLogs (ReferenceTable, ReferenceId, ActivityType, Description, CreatedDate, CreatedBy, IsActive)
            VALUES (@ReferenceTable, @ReferenceId, @ActivityType, @Description, GETDATE(), @CreatedBy, 1);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        var parameters = new DynamicParameters();
        parameters.Add("@ReferenceTable", log.ReferenceTable);
        parameters.Add("@ReferenceId", log.ReferenceId);
        parameters.Add("@ActivityType", log.ActivityType);
        parameters.Add("@Description", log.Description ?? string.Empty);
        parameters.Add("@CreatedBy", log.CreatedBy ?? "System");

        return await _context.ExecuteScalarAsync<int>(sql, parameters);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ActivityLogListViewModel>> GetByReferenceAsync(string referenceTable, int referenceId)
    {
        const string sql = @"
            SELECT LogId, ReferenceTable, ReferenceId, ActivityType, Description, CreatedDate, CreatedBy
            FROM ActivityLogs
            WHERE ReferenceTable = @ReferenceTable AND ReferenceId = @ReferenceId AND IsActive = 1
            ORDER BY CreatedDate DESC";

        return await _context.QueryAsync<ActivityLogListViewModel>(sql, new { ReferenceTable = referenceTable, ReferenceId = referenceId });
    }
}