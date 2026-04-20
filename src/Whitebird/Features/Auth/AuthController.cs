using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Auth.Interfaces;
using Whitebird.Domain.Features.Common.Auth;
using Whitebird.App.Features.Common;

namespace Whitebird.App.Features.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _authService.LoginAsync(request));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request) => this.HandleResult(await _authService.LogoutAsync(request.SessionToken));

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _authService.ForgotPasswordAsync(request.Email));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;

        var sessionToken = GetSessionToken();
        if (string.IsNullOrEmpty(sessionToken)) return Unauthorized();

        var user = await _authService.GetUserBySessionTokenAsync(sessionToken);
        if (!user.IsSuccess || user.Data == null) return Unauthorized();

        return this.HandleResult(await _authService.ResetPasswordAsync(user.Data.UserId, request));
    }

    [HttpPost("reset-password-with-token")]
    public async Task<IActionResult> ResetPasswordWithToken([FromBody] ResetPasswordWithTokenRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _authService.ResetPasswordWithTokenAsync(request));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var sessionToken = GetSessionToken();
        if (string.IsNullOrEmpty(sessionToken)) return Unauthorized();
        return this.HandleResult(await _authService.GetUserBySessionTokenAsync(sessionToken));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;

        var sessionToken = GetSessionToken();
        if (string.IsNullOrEmpty(sessionToken)) return Unauthorized();

        var user = await _authService.GetUserBySessionTokenAsync(sessionToken);
        if (!user.IsSuccess || user.Data == null) return Unauthorized();

        return this.HandleResult(await _authService.ChangePasswordAsync(user.Data.UserId, request));
    }

    [HttpGet("validate-session")]
    public async Task<IActionResult> ValidateSession()
    {
        var sessionToken = GetSessionToken();
        if (string.IsNullOrEmpty(sessionToken)) return Ok(new { isValid = false });
        var result = await _authService.ValidateSessionAsync(sessionToken);
        return Ok(new { isValid = result.IsSuccess });
    }

    private string? GetSessionToken() => Request.Headers["X-Session-Token"].ToString();
}