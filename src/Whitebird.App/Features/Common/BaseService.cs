using Microsoft.Extensions.Logging;
using System.Transactions;

namespace Whitebird.App.Features.Common;

public abstract class BaseService
{
    protected readonly ILogger _logger;

    protected BaseService(ILogger logger)
    {
        _logger = logger;
    }

    // ============================================================
    // EXISTING METHODS (without CancellationToken - for backward compatibility)
    // ============================================================
    
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

    // ============================================================
    // NEW METHODS WITH CancellationToken
    // ============================================================

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

    protected async Task<ServiceResult> ExecuteWithTransactionAsync(
        Func<CancellationToken, Task<ServiceResult>> action,
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
            return ServiceResult.Failure("Operation was cancelled");
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
        Func<CancellationToken, Task<ServiceResult<T>>> action,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await action(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation {OperationName} was cancelled", operationName);
            return ServiceResult<T>.Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {OperationName}", operationName);
            return ServiceResult<T>.Failure($"Failed to {operationName}: {ex.Message}");
        }
    }

    protected async Task<ServiceResult> ExecuteSafelyAsync(
        Func<CancellationToken, Task<ServiceResult>> action,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await action(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation {OperationName} was cancelled", operationName);
            return ServiceResult.Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {OperationName}", operationName);
            return ServiceResult.Failure($"Failed to {operationName}: {ex.Message}");
        }
    }

    // ============================================================
    // ERROR HANDLING HELPERS
    // ============================================================

    /// <summary>
    /// Execute action with try-catch and return default value on error
    /// </summary>
    protected async Task<T> HandleErrorAsync<T>(
        Func<Task<T>> action, 
        T fallbackValue, 
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {OperationName}, returning fallback value", operationName);
            return fallbackValue;
        }
    }

    /// <summary>
    /// Execute action with try-catch and log error without return value
    /// </summary>
    protected async Task<bool> HandleErrorAsync(
        Func<Task> action, 
        string operationName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await action();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {OperationName}", operationName);
            return false;
        }
    }

    /// <summary>
    /// Validate input parameters and return error result if invalid
    /// </summary>
    protected ServiceResult<T> ValidateInput<T>(bool isValid, string errorMessage, string? errorCode = null)
    {
        if (!isValid)
        {
            return ServiceResult<T>.BadRequest(errorMessage);
        }
        return null!;
    }

    /// <summary>
    /// Validate input parameters for non-generic ServiceResult
    /// </summary>
    protected ServiceResult ValidateInput(bool isValid, string errorMessage, string? errorCode = null)
    {
        if (!isValid)
        {
            return ServiceResult.BadRequest(errorMessage);
        }
        return null!;
    }

    /// <summary>
    /// Handle not found scenario
    /// </summary>
    protected ServiceResult<T> NotFound<T>(string entityName, object id)
    {
        return ServiceResult<T>.NotFound($"{entityName} with id {id} not found");
    }

    /// <summary>
    /// Handle conflict scenario
    /// </summary>
    protected ServiceResult<T> Conflict<T>(string message)
    {
        return ServiceResult<T>.Conflict(message);
    }
}