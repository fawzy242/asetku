using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.ActivityLog.View;

namespace Whitebird.Infra.Features.ActivityLog;

public class ActivityLogReps : IActivityLogReps
{
    private readonly DapperContext _context;

    public ActivityLogReps(DapperContext context)
    {
        _context = context;
    }

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

    public async Task<IEnumerable<ActivityLogListDto>> GetByReferenceAsync(string referenceTable, int referenceId)
    {
        const string sql = @"
            SELECT LogId, ReferenceTable, ReferenceId, ActivityType, Description, CreatedDate, CreatedBy
            FROM ActivityLogs
            WHERE ReferenceTable = @ReferenceTable AND ReferenceId = @ReferenceId
            ORDER BY CreatedDate DESC";

        return await _context.QueryAsync<ActivityLogListDto>(sql, new { ReferenceTable = referenceTable, ReferenceId = referenceId });
    }

    public async Task<IEnumerable<ActivityLogListDto>> GetByActivityTypeAsync(string activityType, int limit = 100)
    {
        const string sql = @"
            SELECT TOP (@Limit) LogId, ReferenceTable, ReferenceId, ActivityType, Description, CreatedDate, CreatedBy
            FROM ActivityLogs
            WHERE ActivityType = @ActivityType
            ORDER BY CreatedDate DESC";

        return await _context.QueryAsync<ActivityLogListDto>(sql, new { ActivityType = activityType, Limit = limit });
    }

    public async Task<IEnumerable<ActivityLogListDto>> GetRecentAsync(int limit = 50)
    {
        const string sql = @"
            SELECT TOP (@Limit) LogId, ReferenceTable, ReferenceId, ActivityType, Description, CreatedDate, CreatedBy
            FROM ActivityLogs
            ORDER BY CreatedDate DESC";

        return await _context.QueryAsync<ActivityLogListDto>(sql, new { Limit = limit });
    }

    public async Task<IEnumerable<ActivityLogListDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT LogId, ReferenceTable, ReferenceId, ActivityType, Description, CreatedDate, CreatedBy
            FROM ActivityLogs
            WHERE CreatedDate BETWEEN @StartDate AND @EndDate
            ORDER BY CreatedDate DESC";

        return await _context.QueryAsync<ActivityLogListDto>(sql, new { StartDate = startDate, EndDate = endDate });
    }
}