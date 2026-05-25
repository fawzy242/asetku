using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Whitebird.App.Features.Auth;

namespace Whitebird.Middleware;

public class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IAuthService _authService;

    public SessionAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IAuthService authService)
        : base(options, logger, encoder, clock)
    {
        _authService = authService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Session-Token", out var sessionTokenValues))
        {
            return AuthenticateResult.NoResult();
        }

        var sessionToken = sessionTokenValues.ToString();
        if (string.IsNullOrEmpty(sessionToken))
        {
            return AuthenticateResult.NoResult();
        }

        var userResult = await _authService.GetUserBySessionTokenAsync(sessionToken);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return AuthenticateResult.Fail("Invalid or expired session token");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userResult.Data.UserId.ToString()),
            new Claim(ClaimTypes.Name, userResult.Data.Username ?? string.Empty),
            new Claim(ClaimTypes.Email, userResult.Data.Email ?? string.Empty),
            new Claim("FullName", userResult.Data.FullName ?? string.Empty),
            new Claim(ClaimTypes.Role, userResult.Data.RoleId ?? "User")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}