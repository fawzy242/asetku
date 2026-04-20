namespace Whitebird.App.Features.Common.Service;

public interface IActivityLogService
{
    Task LogCreateAsync(string referenceTable, int referenceId, string description, string? createdBy = null);
    Task LogUpdateAsync(string referenceTable, int referenceId, string description, string? updatedBy = null);
    Task LogDeleteAsync(string referenceTable, int referenceId, string description, string? deletedBy = null);
    Task LogSoftDeleteAsync(string referenceTable, int referenceId, string description, string? deletedBy = null);
    Task LogAsync(string referenceTable, int referenceId, string activityType, string description, string? createdBy = null);
    Task LogErrorAsync(string referenceTable, int referenceId, string operation, Exception ex, string? createdBy = null);
    Task LogAsyncSafe(string referenceTable, int referenceId, string activityType, string description, string? createdBy = null);
}