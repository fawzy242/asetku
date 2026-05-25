using Microsoft.Extensions.Logging;
using System.Transactions;

namespace Whitebird.App.Features.Common;

public abstract class BaseServiceWithCancellation : BaseService
{
    protected BaseServiceWithCancellation(ILogger logger) : base(logger)
    {
    }

    protected async Task<ServiceResult<T>> ExecuteWithTransactionAsync<T>(
        Func<CancellationToken, Task<ServiceResult<T>>> action,
        string operationName,
        CancellationToken cancellationToken = default,
        Func<Exception, Task>? onError = null,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = isolationLevel },
            TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            var result = await action(cancellationToken);
            if (result.IsSuccess)
            {
                scope.Complete();
            }
            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation {OperationName} was cancelled", operationName);
            return ServiceResult<T>.Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {OperationName}", operationName);

            if (onError != null)
            {
                try
                {
                    await onError(ex);
                }
                catch (Exception logEx)
                {
                    _logger.LogWarning(logEx, "Failed to log error for {OperationName}", operationName);
                }
            }

            return ServiceResult<T>.Failure($"Failed to {operationName}: {ex.Message}");
        }
    }
}