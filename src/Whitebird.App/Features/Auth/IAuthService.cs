using Microsoft.AspNetCore.Http;
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
    /// <param name="request">Login credentials</param>
    /// <returns>Login response with session token and user info</returns>
    Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);

    /// <summary>
    /// Logs out a user by invalidating their session
    /// </summary>
    /// <param name="sessionToken">Session token to invalidate</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> LogoutAsync(string sessionToken);

    /// <summary>
    /// Initiates password reset process by sending a reset code to user's email
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> ForgotPasswordAsync(string email);

    /// <summary>
    /// Resets password using current password verification
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Reset password data (current + new password)</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> ResetPasswordAsync(int userId, ResetPasswordRequest request);

    /// <summary>
    /// Resets password using email and reset token (from forgot password)
    /// </summary>
    /// <param name="request">Reset password with token data</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> ResetPasswordWithTokenAsync(ResetPasswordWithTokenRequest request);

    /// <summary>
    /// Gets user information by user ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User DTO or not found result</returns>
    Task<ServiceResult<UserDto>> GetUserByIdAsync(int userId);

    /// <summary>
    /// Gets user information by session token
    /// </summary>
    /// <param name="sessionToken">Session token</param>
    /// <returns>User DTO or not found result</returns>
    Task<ServiceResult<UserDto>> GetUserBySessionTokenAsync(string sessionToken);

    /// <summary>
    /// Changes user password (requires old password verification)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Change password data</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordRequest request);

    /// <summary>
    /// Validates if a session token is still active
    /// </summary>
    /// <param name="sessionToken">Session token to validate</param>
    /// <returns>Success if valid, failure if expired or invalid</returns>
    Task<ServiceResult> ValidateSessionAsync(string sessionToken);

    // ============================================================
    // PROFILE PHOTO
    // ============================================================

    /// <summary>
    /// Uploads a profile photo for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="file">Image file (max 5MB, jpg/png/gif/webp)</param>
    /// <returns>URL of uploaded photo</returns>
    Task<ServiceResult<string>> UploadProfilePhotoAsync(int userId, IFormFile file);

    /// <summary>
    /// Gets profile photo bytes for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Image file bytes</returns>
    Task<ServiceResult<byte[]>> GetProfilePhotoAsync(int userId);

    /// <summary>
    /// Deletes a user's profile photo
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> DeleteProfilePhotoAsync(int userId);

    // ============================================================
    // PROFILE UPDATE
    // ============================================================

    /// <summary>
    /// Updates user profile information (name, email, phone)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Profile update data</param>
    /// <returns>Updated user DTO</returns>
    Task<ServiceResult<UserDto>> UpdateProfileAsync(int userId, UpdateProfileRequest request);
}