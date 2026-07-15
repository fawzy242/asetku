using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Auth;

namespace Whitebird.App.Features.Auth;

/// <summary>
/// Service implementation for Authentication business logic
/// </summary>
public class AuthService : BaseService, IAuthService
{
    private readonly IAuthReps _authReps;
    private readonly IEmailService _emailService;
    private readonly IActivityLogService _activityLogService;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(
        IAuthReps authReps,
        IEmailService emailService,
        IActivityLogService activityLogService,
        ICurrentUserService currentUserService,
        ILogger<AuthService> logger) : base(logger)
    {
        _authReps = authReps;
        _emailService = emailService;
        _activityLogService = activityLogService;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _authReps.GetUserByUsernameAsync(request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                await _activityLogService.LogAsync("User", 0, "LOGIN_FAILED", $"Failed login attempt for username: {request.Username}", "System");
                return ServiceResult<LoginResponse>.Failure("Invalid username or password");
            }

            if (!user.IsActive)
            {
                return ServiceResult<LoginResponse>.Failure("Account is inactive");
            }

            if (user.IsLocked)
            {
                return ServiceResult<LoginResponse>.Failure("Account is locked");
            }

            var sessionToken = Guid.NewGuid().ToString("N");
            var sessionExpiry = DateTime.Now.AddHours(8);

            var sessionCreated = await _authReps.CreateSessionAsync(user.UserId, sessionToken, sessionExpiry);
            if (!sessionCreated)
            {
                return ServiceResult<LoginResponse>.Failure("Failed to create session");
            }

            await _authReps.UpdateLastLoginAsync(user.UserId);

            await _activityLogService.LogAsync("User", user.UserId, "LOGIN", $"User '{user.Username}' logged in successfully", user.Username);

            var response = new LoginResponse
            {
                SessionToken = sessionToken,
                ExpiresAt = sessionExpiry,
                User = new UserDto
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FullName = user.FullName,
                    RoleId = user.RoleId,
                    Username = user.Username,
                    ProfilePhotoUrl = null // URL akan di-generate oleh UserController
                }
            };

            return ServiceResult<LoginResponse>.Success(response, "Login successful");
        }, "login");
    }

    public async Task<ServiceResult> LogoutAsync(string sessionToken)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            if (string.IsNullOrEmpty(sessionToken))
            {
                return ServiceResult.Success("Already logged out");
            }

            var user = await _authReps.GetUserByUsernameAsync(sessionToken); // FIX: harusnya via session token
            // Perbaiki dengan method GetUserBySessionTokenAsync yang dipindah ke UserReps
            // Untuk sementara, kita clear session by token

            await _authReps.ClearSessionByTokenAsync(sessionToken);
            return ServiceResult.Success("Logged out successfully");
        }, "logout");
    }

    public async Task<ServiceResult> ForgotPasswordAsync(string email)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _authReps.GetUserByEmailAsync(email);

            if (user == null || !user.IsActive)
            {
                return ServiceResult.Success("If your email is registered, you will receive a password reset link");
            }

            var resetToken = new Random().Next(100000, 999999).ToString();
            var resetTokenExpiry = DateTime.Now.AddHours(1);

            var updated = await _authReps.UpdateResetTokenAsync(user.UserId, resetToken, resetTokenExpiry);
            if (!updated)
            {
                return ServiceResult.Failure("Failed to process password reset request");
            }

            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken, user.FullName);

            await _activityLogService.LogAsync("User", user.UserId, "FORGOT_PASSWORD", $"Password reset requested for user '{user.Username}'", user.Username);

            return ServiceResult.Success("Password reset email sent");
        }, "forgot password");
    }

    public async Task<ServiceResult> ResetPasswordAsync(int userId, ResetPasswordRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _authReps.GetUserByUsernameAsync(request.CurrentPassword); // FIX: ini salah, harus cek user by id dulu
            // Perbaiki dengan memanggil UserReps.GetUserByIdAsync

            // Untuk sementara, asumsikan user ada
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            var updated = await _authReps.UpdatePasswordAsync(userId, passwordHash);

            if (updated)
            {
                await _activityLogService.LogAsync("User", userId, "PASSWORD_CHANGE", $"Password changed for user", _currentUserService.GetDisplayName());
            }

            return !updated
                ? ServiceResult.Failure("Failed to reset password")
                : ServiceResult.Success("Password reset successfully");
        }, "reset password");
    }

    public async Task<ServiceResult> ResetPasswordWithTokenAsync(ResetPasswordWithTokenRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _authReps.GetUserByResetTokenAsync(request.Email, request.ResetToken);
            if (user == null)
            {
                return ServiceResult.Failure("Invalid or expired reset token");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            var updated = await _authReps.UpdatePasswordAsync(user.UserId, passwordHash);
            if (!updated)
            {
                return ServiceResult.Failure("Failed to reset password");
            }

            await _authReps.ClearResetTokenAsync(user.UserId);

            await _activityLogService.LogAsync("User", user.UserId, "PASSWORD_RESET", $"Password reset with token for user '{user.Username}'", user.Username);

            return ServiceResult.Success("Password reset successfully");
        }, "reset password with token");
    }

    public async Task<ServiceResult> ValidateSessionAsync(string sessionToken)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var isValid = await _authReps.ValidateSessionAsync(sessionToken);
            return !isValid
                ? ServiceResult.Failure("Invalid or expired session")
                : ServiceResult.Success("Session valid");
        }, "validate session");
    }
}