using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace Whitebird.Middleware;

public class AuthRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, LoginAttemptInfo> _attempts = new();

    public AuthRateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/Auth/login"))
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var key = $"{ipAddress}";

            if (_attempts.TryGetValue(key, out var attempt))
            {
                if (attempt.FailedCount >= 5 && attempt.LastAttempt > DateTime.UtcNow.AddMinutes(-5))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.ContentType = "application/json";
                    var errorResponse = new { error = "Too many login attempts. Please try again later." };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                    return;
                }
            }

            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            if (context.Response.StatusCode == 401)
            {
                if (!_attempts.ContainsKey(key))
                    _attempts[key] = new LoginAttemptInfo();

                _attempts[key].FailedCount++;
                _attempts[key].LastAttempt = DateTime.UtcNow;
            }
            else if (context.Response.StatusCode == 200)
            {
                _attempts.TryRemove(key, out _);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
        else
        {
            await _next(context);
        }
    }
}

public class LoginAttemptInfo
{
    public int FailedCount { get; set; }
    public DateTime LastAttempt { get; set; }
}