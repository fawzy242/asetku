using Microsoft.Extensions.Logging;

namespace Whitebird.App.Features.Common;

public static class ErrorHandler
{
    public static async Task<T> HandleAsync<T>(
        Func<Task<T>> action,
        ILogger logger,
        string operationName,
        T fallbackValue = default!)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {OperationName}: {Message}", operationName, ex.Message);
            return fallbackValue;
        }
    }

    public static async Task<bool> HandleAsync(
        Func<Task> action,
        ILogger logger,
        string operationName)
    {
        try
        {
            await action();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {OperationName}: {Message}", operationName, ex.Message);
            return false;
        }
    }

    public static T HandleSync<T>(
        Func<T> action,
        ILogger logger,
        string operationName,
        T fallbackValue = default!)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {OperationName}: {Message}", operationName, ex.Message);
            return fallbackValue;
        }
    }
}