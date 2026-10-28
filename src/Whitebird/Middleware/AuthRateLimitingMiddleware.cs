using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;

namespace Whitebird.Middleware;

/// <summary>
/// Per-IP throttle for authentication-related endpoints.
///
/// Policies:
///   * /api/Auth/login                     — 5 failures / 5 min
///   * /api/Auth/reset-password-with-token — 10 failures / 15 min
///   * /api/Auth/forgot-password           — 5 requests / 15 min (all requests)
///
/// Failure semantics per policy:
///   * <c>CountAllRequests = false</c> — increment only when the downstream status
///     code is listed in <c>FailureCodes</c>. Used for login and reset-with-token.
///   * <c>CountAllRequests = true</c>  — increment on every non-throttled request.
///     Used for forgot-password (each request may trigger an outbound email).
///
/// Counter lifecycle:
///   * On success (200) the entry is removed only if <c>ClearOnSuccess = true</c>.
///   * When the previous attempt's <c>LastAttempt</c> is outside the policy
///     window, the next attempt resets the counter to 1 rather than incrementing.
///     This closes the "pathological growth" case where spaced-out retries kept
///     extending a blocked state indefinitely (also present in the pre-H5 design).
///   * Entries idle beyond <see cref="_sweepEvictionAge"/> are evicted by an
///     opportunistic sweep throttled to once per minute.
///
/// Response:
///   * 429 Too Many Requests + JSON body { error: "..." } + standard
///     Retry-After header (seconds until the current block expires).
///
/// Failure codes rationale (H4):
///   * /api/Auth/login is [AllowAnonymous]. Authentication handler is never
///     invoked, so it never produces 401. Failures come from ControllerHelper
///     mapping ErrorType "Error" → 400. Pre-H4 the middleware only counted 401
///     and was therefore a no-op. Counting 400 (and 401 defensively) fixes that.
///   * reset-password-with-token and forgot-password are also [AllowAnonymous].
///     Failures → 400. forgot-password success → 200 (non-enumerating) but is
///     counted via CountAllRequests.
/// </summary>
public class AuthRateLimitingMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly ConcurrentDictionary<string, LoginAttemptInfo> _attempts = new();

    private static readonly Dictionary<string, PathPolicy> _policies = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/api/Auth/login"] = new PathPolicy
        {
            CountAllRequests = false,
            FailureCodes = new[] { 400, 401 },
            MaxAttempts = 5,
            Window = TimeSpan.FromMinutes(5),
            ClearOnSuccess = true,
            Message = "Too many login attempts. Please try again later."
        },
        ["/api/Auth/reset-password-with-token"] = new PathPolicy
        {
            CountAllRequests = false,
            FailureCodes = new[] { 400 },
            MaxAttempts = 10,
            Window = TimeSpan.FromMinutes(15),
            ClearOnSuccess = false,
            Message = "Too many password reset attempts. Please try again later."
        },
        ["/api/Auth/forgot-password"] = new PathPolicy
        {
            // Each accepted request may trigger an outbound email; count every
            // request (200 or 400) so an attacker cannot flood the SMTP channel
            // or spam a victim by spamming the endpoint.
            CountAllRequests = true,
            FailureCodes = Array.Empty<int>(),
            MaxAttempts = 5,
            Window = TimeSpan.FromMinutes(15),
            // Do not clear on 200 — a success here is still an email send.
            ClearOnSuccess = false,
            Message = "Too many password reset requests. Please try again later."
        }
    };

    private static readonly TimeSpan _sweepInterval = TimeSpan.FromMinutes(1);

    // Derived so adding a wider policy window does not break the eviction age.
    private static readonly TimeSpan _sweepEvictionAge = ComputeSweepEvictionAge();

    private static long _lastSweepTicks;

    public AuthRateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!TryMatchPolicy(context.Request.Path, out var pathKey, out var policy))
        {
            await _next(context);
            return;
        }

        MaybeEvictStaleEntries();

        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"{ipAddress}|{pathKey}";

        if (_attempts.TryGetValue(key, out var attempt))
        {
            if (attempt.FailedCount >= policy.MaxAttempts && attempt.LastAttempt > DateTime.UtcNow - policy.Window)
            {
                var remaining = attempt.LastAttempt + policy.Window - DateTime.UtcNow;
                var retryAfterSeconds = (int)Math.Ceiling(remaining.TotalSeconds);
                if (retryAfterSeconds < 1) retryAfterSeconds = 1;

                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";
                context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = policy.Message }));
                return;
            }
        }

        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        try
        {
            await _next(context);
        }
        finally
        {
            var status = context.Response.StatusCode;

            var isAttempt =
                policy.CountAllRequests ||
                Array.IndexOf(policy.FailureCodes, status) >= 0;

            if (isAttempt)
            {
                // Reset-on-stale: if the previous attempt is outside the policy
                // window, start over at 1 instead of incrementing. Prevents a
                // caller who respects the block (waits out the window, retries)
                // from being re-blocked immediately because of an accumulated
                // counter from earlier attempts.
                var now = DateTime.UtcNow;
                var cutoff = now - policy.Window;

                _attempts.AddOrUpdate(
                    key,
                    _ => new LoginAttemptInfo { FailedCount = 1, LastAttempt = now },
                    (_, existing) => new LoginAttemptInfo
                    {
                        FailedCount = existing.LastAttempt > cutoff
                            ? existing.FailedCount + 1
                            : 1,
                        LastAttempt = now
                    });
            }
            else if (status == 200 && policy.ClearOnSuccess)
            {
                _attempts.TryRemove(key, out _);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
    }

    private static bool TryMatchPolicy(PathString path, out string pathKey, out PathPolicy policy)
    {
        foreach (var kvp in _policies)
        {
            if (path.StartsWithSegments(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                pathKey = kvp.Key;
                policy = kvp.Value;
                return true;
            }
        }

        pathKey = string.Empty;
        policy = null!;
        return false;
    }

    private static TimeSpan ComputeSweepEvictionAge()
    {
        var maxWindow = TimeSpan.Zero;
        foreach (var policy in _policies.Values)
        {
            if (policy.Window > maxWindow) maxWindow = policy.Window;
        }
        return maxWindow + _sweepInterval;
    }

    private static void MaybeEvictStaleEntries()
    {
        var nowTicks = DateTime.UtcNow.Ticks;
        var lastTicks = Interlocked.Read(ref _lastSweepTicks);

        if (lastTicks != 0 && nowTicks - lastTicks < _sweepInterval.Ticks)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref _lastSweepTicks, nowTicks, lastTicks) != lastTicks)
        {
            return;
        }

        var cutoff = DateTime.UtcNow - _sweepEvictionAge;

        foreach (var kvp in _attempts)
        {
            if (kvp.Value.LastAttempt < cutoff)
            {
                _attempts.TryRemove(kvp.Key, out _);
            }
        }
    }

    private sealed class PathPolicy
    {
        public bool CountAllRequests { get; init; }
        public int[] FailureCodes { get; init; } = Array.Empty<int>();
        public int MaxAttempts { get; init; }
        public TimeSpan Window { get; init; }
        public bool ClearOnSuccess { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}

public class LoginAttemptInfo
{
    public int FailedCount { get; set; }
    public DateTime LastAttempt { get; set; }
}