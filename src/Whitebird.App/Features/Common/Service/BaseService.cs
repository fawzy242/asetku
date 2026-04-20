using Microsoft.Extensions.Logging;
using System.Transactions;

namespace Whitebird.App.Features.Common.Service;

public abstract class BaseService
{
    protected readonly ILogger _logger;

    protected BaseService(ILogger logger)
    {
        _logger = logger;
    }

    protected async Task<ServiceResult<T>> ExecuteWithTransactionAsync<T>(
        Func<Task<ServiceResult<T>>> action,
        string operationName,
        Func<Exception, Task>? onError = null,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = isolationLevel },
            TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            var result = await action();
            if (result.IsSuccess)
            {
                scope.Complete();
            }
            return result;
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

    protected async Task<ServiceResult> ExecuteWithTransactionAsync(
        Func<Task<ServiceResult>> action,
        string operationName,
        Func<Exception, Task>? onError = null,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = isolationLevel },
            TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            var result = await action();
            if (result.IsSuccess)
            {
                scope.Complete();
            }
            return result;
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

            return ServiceResult.Failure($"Failed to {operationName}: {ex.Message}");
        }
    }

    protected async Task<ServiceResult<T>> ExecuteSafelyAsync<T>(
        Func<Task<ServiceResult<T>>> action,
        string operationName)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {OperationName}", operationName);
            return ServiceResult<T>.Failure($"Failed to {operationName}: {ex.Message}");
        }
    }

    protected async Task<ServiceResult> ExecuteSafelyAsync(
        Func<Task<ServiceResult>> action,
        string operationName)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {OperationName}", operationName);
            return ServiceResult.Failure($"Failed to {operationName}: {ex.Message}");
        }
    }
}