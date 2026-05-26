using Microsoft.Extensions.Logging;
using Whitebird.Domain.Features.ActivityLog;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.ActivityLog;

namespace Whitebird.App.Features.Common;

public class ActivityLogService : IActivityLogService
{
    private readonly IActivityLogReps _activityLogReps;
    private readonly ILogger<ActivityLogService> _logger;

    public ActivityLogService(IActivityLogReps activityLogReps, ILogger<ActivityLogService> logger)
    {
        _activityLogReps = activityLogReps;
        _logger = logger;
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

    public async Task LogAsyncSafe(string referenceTable, int referenceId, string activityType, string description, string? createdBy = null)
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
            _logger.LogWarning(ex, "Failed to insert activity log (safe mode) for {Table}.{Id}",
                referenceTable, referenceId);
        }
    }

    // NEW: Safe async wrapper for any action
    public async Task<T?> ExecuteSafeAsync<T>(Func<Task<T>> action, T? fallback = default)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing safe action");
            return fallback;
        }
    }

    public async Task ExecuteSafeAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing safe action");
        }
    }
}