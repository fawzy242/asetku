using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Common;

namespace Whitebird.App.Features.Auth;

/// <summary>
/// Service interface for Authentication business logic
/// </summary>
public interface IAuthService
{
    // ============================================================
    // AUTHENTICATION
    // ============================================================

    /// <summary>
    /// Authenticates a user and creates a session
    /// </summary>
    Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);

    /// <summary>
    /// Logs out a user by invalidating their session
    /// </summary>
    Task<ServiceResult> LogoutAsync(string sessionToken);

    /// <summary>
    /// Initiates password reset process by sending a reset code to user's email
    /// </summary>
    Task<ServiceResult> ForgotPasswordAsync(string email);

    /// <summary>
    /// Resets password using current password verification
    /// </summary>
    Task<ServiceResult> ResetPasswordAsync(int userId, ResetPasswordRequest request);

    /// <summary>
    /// Resets password using email and reset token (from forgot password)
    /// </summary>
    Task<ServiceResult> ResetPasswordWithTokenAsync(ResetPasswordWithTokenRequest request);

    /// <summary>
    /// Validates if a session token is still active
    /// </summary>
    Task<ServiceResult> ValidateSessionAsync(string sessionToken);
}