using Polly;
using Polly.Retry;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Whitebird.Infra.Resilience;

public static class PollyRetryPolicy
{
    public static AsyncRetryPolicy CreateRetryPolicy(ILogger logger, int maxRetries = 3, int retryDelaySeconds = 30)
    {
        return Policy
            .Handle<SqlException>(ex => ex.Number == 1205 || ex.Number == 1222 || ex.Number == -2)
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: maxRetries,  // <-- CHANGE THIS: 'maxRetryCount' to 'retryCount'
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * 2),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(exception,
                        "Retry {RetryCount} after {DelayMs}ms due to {ExceptionType}",
                        retryCount, timeSpan.TotalMilliseconds, exception.GetType().Name);
                });
    }
}