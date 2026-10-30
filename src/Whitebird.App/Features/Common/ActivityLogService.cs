using Microsoft.Extensions.Logging;
using Whitebird.Domain.Features.ActivityLog;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.ActivityLog;

namespace Whitebird.App.Features.Common;

/// <summary>
/// Service for logging activities across the application
/// </summary>
public class ActivityLogService : BaseService, IActivityLogService
{
    private readonly IActivityLogReps _activityLogReps;

    public ActivityLogService(IActivityLogReps activityLogReps, ILogger<ActivityLogService> logger)
        : base(logger)
    {
        _activityLogReps = activityLogReps;
    }

    public async Task LogCreateAsync(string referenceTable, int referenceId, string description, string? createdBy = null)
    {
        await LogAsync(referenceTable, referenceId, "CREATE", description, createdBy);
    }

    public async Task LogUpdateAsync(string referenceTable, int referenceId, string description, string? updatedBy = null)
    {
        await LogAsync(referenceTable, referenceId, "UPDATE", description, updatedBy);
    }

    public async Task LogDeleteAsync(string referenceTable, int referenceId, string description, string? deletedBy = null)
    {
        await LogAsync(referenceTable, referenceId, "DELETE", description, deletedBy);
    }

    public async Task LogSoftDeleteAsync(string referenceTable, int referenceId, string description, string? deletedBy = null)
    {
        await LogAsync(referenceTable, referenceId, "SOFT_DELETE", description, deletedBy);
    }

    public async Task LogAsync(string referenceTable, int referenceId, string activityType, string description, string? createdBy = null)
    {
        try
        {
            var log = new CreateActivityLogDto
            {
                ReferenceTable = referenceTable,
                ReferenceId = referenceId,
                ActivityType = activityType,
                Description = description,
                CreatedBy = createdBy ?? "System"
            };

            await _activityLogReps.InsertAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to insert activity log for {Table}.{Id} - {Type}",
                referenceTable, referenceId, activityType);
        }
    }

    public async Task LogErrorAsync(string referenceTable, int referenceId, string operation, Exception ex, string? createdBy = null)
    {
        var description = $"Error during {operation}: {ex.Message}";
        await LogAsync(referenceTable, referenceId, "ERROR", description, createdBy);
    }

    // P4-1: kept as a public interface member for callers that opted into the
    // "safe" naming, but it is now a one-line delegation to LogAsync — the two
    // bodies were previously identical.
    public Task LogAsyncSafe(string referenceTable, int referenceId, string activityType, string description, string? createdBy = null)
        => LogAsync(referenceTable, referenceId, activityType, description, createdBy);

    public async Task<ServiceResult<IEnumerable<ActivityLogListViewModel>>> GetByReferenceAsync(string referenceTable, int referenceId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(referenceTable))
            {
                return ServiceResult<IEnumerable<ActivityLogListViewModel>>.BadRequest("referenceTable is required");
            }

            if (referenceId <= 0)
            {
                return ServiceResult<IEnumerable<ActivityLogListViewModel>>.BadRequest("referenceId must be greater than 0");
            }

            var data = await _activityLogReps.GetByReferenceAsync(referenceTable, referenceId);
            return ServiceResult<IEnumerable<ActivityLogListViewModel>>.Success(data);
        }, "get activity logs by reference");
    }
}