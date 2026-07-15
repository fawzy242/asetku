using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.User;
using Whitebird.Domain.Features.Common;
using Whitebird.Features.Common;

namespace Whitebird.Features.User;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // ============================================================
    // PROFILE
    // ============================================================

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var sessionToken = GetSessionToken();
        if (string.IsNullOrEmpty(sessionToken))
            return Unauthorized();

        return this.HandleResult(await _userService.GetUserBySessionTokenAsync(sessionToken));
    }

    /// <summary>
    /// Update user profile (FullName, Email, PhoneNumber)
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;

        var sessionToken = GetSessionToken();
        if (string.IsNullOrEmpty(sessionToken)) return Unauthorized();

        var userResult = await _userService.GetUserBySessionTokenAsync(sessionToken);
        if (!userResult.IsSuccess || userResult.Data == null) return Unauthorized();

        return this.HandleResult(await _userService.UpdateProfileAsync(userResult.Data.UserId, request));
    }

    // ============================================================
    // CHANGE PASSWORD
    // ============================================================

    /// <summary>
    /// Change password (requires old password)
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var validation = this.HandleModelState();
        if (validation != null) return validation;

        var sessionToken = GetSessionToken();
        if (string.IsNullOrEmpty(sessionToken)) return Unauthorized();

        var user = await _userService.GetUserBySessionTokenAsync(sessionToken);
        if (!user.IsSuccess || user.Data == null) return Unauthorized();

        return this.HandleResult(await _userService.ChangePasswordAsync(user.Data.UserId, request));
    }

    // ============================================================
    // PHOTO PROFILE
    // ============================================================

    /// <summary>
    /// Upload profile photo (max 5MB, jpg/png/gif/webp)
    /// </summary>
    [HttpPost("profile-photo")]
    public async Task<IActionResult> UploadProfilePhoto(IFormFile file)
    {
        var sessionToken = GetSessionToken();
        if (string.IsNullOrEmpty(sessionToken))
            return Unauthorized();

        var user = await _userService.GetUserBySessionTokenAsync(sessionToken);
        if (!user.IsSuccess || user.Data == null)
            return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        var result = await _userService.UploadProfilePhotoAsync(user.Data.UserId, file);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Get profile photo
    /// </summary>
    [HttpGet("profile-photo")]
    public async Task<IActionResult> GetProfilePhoto()
    {
        var sessionToken = GetSessionToken();
        if (string.IsNullOrEmpty(sessionToken))
            return Unauthorized();

        var user = await _userService.GetUserBySessionTokenAsync(sessionToken);
        if (!user.IsSuccess || user.Data == null)
            return Unauthorized();

        var result = await _userService.GetProfilePhotoAsync(user.Data.UserId);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        return File(result.Data, "image/jpeg");
    }

    /// <summary>
    /// Delete profile photo
    /// </summary>
    [HttpDelete("profile-photo")]
    public async Task<IActionResult> DeleteProfilePhoto()
    {
        var sessionToken = GetSessionToken();
        if (string.IsNullOrEmpty(sessionToken))
            return Unauthorized();

        var user = await _userService.GetUserBySessionTokenAsync(sessionToken);
        if (!user.IsSuccess || user.Data == null)
            return Unauthorized();

        var result = await _userService.DeleteProfilePhotoAsync(user.Data.UserId);
        return this.HandleResult(result);
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================

    private string? GetSessionToken()
    {
        return Request.Headers["X-Session-Token"].ToString();
    }
}