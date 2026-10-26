using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Whitebird.App.Features.User;

namespace Whitebird.Middleware;

public class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IUserService _userService;

    public SessionAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IUserService userService)
        : base(options, logger, encoder)
    {
        _userService = userService;
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

        var userResult = await _userService.GetUserBySessionTokenAsync(sessionToken);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            return AuthenticateResult.Fail("Invalid or expired session token");
        }

        // Role claim now carries the resolved RoleName from UserRoles/Roles.
        // Falls back to "User" when the authenticated user has no assigned role.
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userResult.Data.UserId.ToString()),
            new Claim(ClaimTypes.Name, userResult.Data.Username ?? string.Empty),
            new Claim(ClaimTypes.Email, userResult.Data.Email ?? string.Empty),
            new Claim("FullName", userResult.Data.FullName ?? string.Empty),
            new Claim(ClaimTypes.Role, userResult.Data.RoleName ?? "User")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}