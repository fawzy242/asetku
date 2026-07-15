using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Auth;
using Whitebird.Domain.Features.Common;
using Whitebird.Features.Common;

namespace Whitebird.Features.Auth;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _authService.LoginAsync(request));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        return this.HandleResult(await _authService.LogoutAsync(request.SessionToken));
    }

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

        // Get user via UserService
        // Untuk sementara, kita perlu UserService di sini
        // Atau menggunakan AuthService dengan parameter userId

        // Sementara return Unauthorized
        return Unauthorized();
    }

    [HttpPost("reset-password-with-token")]
    public async Task<IActionResult> ResetPasswordWithToken([FromBody] ResetPasswordWithTokenRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;
        return this.HandleResult(await _authService.ResetPasswordWithTokenAsync(request));
    }

    [HttpGet("validate-session")]
    public async Task<IActionResult> ValidateSession()
    {
        var sessionToken = GetSessionToken();
        if (string.IsNullOrEmpty(sessionToken)) return Ok(new { isValid = false });
        var result = await _authService.ValidateSessionAsync(sessionToken);
        return Ok(new { isValid = result.IsSuccess });
    }

    private string? GetSessionToken()
    {
        return Request.Headers["X-Session-Token"].ToString();
    }
}