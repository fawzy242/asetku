using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Auth.Interfaces;
using Whitebird.App.Features.Common.Service;
using Whitebird.Domain.Features.Common.Auth;
using Whitebird.Infra.Features.Auth;
using BCrypt.Net;

namespace Whitebird.App.Features.Auth.Service;

public class AuthService : BaseService, IAuthService
{
    private readonly IAuthReps _authRepository;
    private readonly IEmailService _emailService;
    private readonly IActivityLogService _activityLogService;

    public AuthService(
        IAuthReps authRepository,
        IEmailService emailService,
        IActivityLogService activityLogService,
        ILogger<AuthService> logger) : base(logger)
    {
        _authRepository = authRepository;
        _emailService = emailService;
        _activityLogService = activityLogService;
    }

    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _authRepository.GetUserByEmailAsync(request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                await _activityLogService.LogAsync("User", 0, "LOGIN_FAILED", $"Failed login attempt for email: {request.Email}", "System");
                return ServiceResult<LoginResponse>.Failure("Invalid email or password");
            }

            if (!user.IsActive)
                return ServiceResult<LoginResponse>.Failure("Account is inactive");
            if (user.IsLocked)
                return ServiceResult<LoginResponse>.Failure("Account is locked");

            var sessionToken = Guid.NewGuid().ToString("N");
            var sessionExpiry = DateTime.Now.AddHours(8);

            await _authRepository.CreateSessionAsync(user.UserId, sessionToken, sessionExpiry);
            await _authRepository.UpdateLastLoginAsync(user.UserId);

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
                    Username = user.Username
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
                return ServiceResult.Success("Already logged out");

            var user = await _authRepository.GetUserBySessionTokenAsync(sessionToken);
            if (user != null)
            {
                await _activityLogService.LogAsync("User", user.UserId, "LOGOUT", $"User '{user.Username}' logged out", user.Username);
            }

            await _authRepository.ClearSessionByTokenAsync(sessionToken);
            return ServiceResult.Success("Logged out successfully");
        }, "logout");
    }

    public async Task<ServiceResult> ForgotPasswordAsync(string email)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _authRepository.GetUserByEmailAsync(email);

            if (user == null || !user.IsActive)
                return ServiceResult.Success("If your email is registered, you will receive a password reset link");

            var resetToken = new Random().Next(100000, 999999).ToString();
            var resetTokenExpiry = DateTime.Now.AddHours(1);

            var updated = await _authRepository.UpdateResetTokenAsync(user.UserId, resetToken, resetTokenExpiry);
            if (!updated)
                return ServiceResult.Failure("Failed to process password reset request");

            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken, user.FullName);

            await _activityLogService.LogAsync("User", user.UserId, "FORGOT_PASSWORD", $"Password reset requested for user '{user.Username}'", user.Username);

            return ServiceResult.Success("Password reset email sent");
        }, "forgot password");
    }

    public async Task<ServiceResult> ResetPasswordAsync(int userId, ResetPasswordRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null || !user.IsActive)
                return ServiceResult.Failure("User not found");
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return ServiceResult.Failure("Current password is incorrect");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            var updated = await _authRepository.UpdatePasswordAsync(userId, passwordHash);

            if (updated)
            {
                await _activityLogService.LogAsync("User", userId, "PASSWORD_CHANGE", $"Password changed for user '{user.Username}'", user.Username);
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
            var user = await _authRepository.GetUserByResetTokenAsync(request.Email, request.ResetToken);
            if (user == null)
                return ServiceResult.Failure("Invalid or expired reset token");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            var updated = await _authRepository.UpdatePasswordAsync(user.UserId, passwordHash);
            if (!updated)
                return ServiceResult.Failure("Failed to reset password");

            await _authRepository.ClearResetTokenAsync(user.UserId);

            await _activityLogService.LogAsync("User", user.UserId, "PASSWORD_RESET", $"Password reset with token for user '{user.Username}'", user.Username);

            return ServiceResult.Success("Password reset successfully");
        }, "reset password with token");
    }

    public async Task<ServiceResult<UserDto>> GetUserByIdAsync(int userId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null || !user.IsActive)
                return ServiceResult<UserDto>.NotFound("User not found");

            return ServiceResult<UserDto>.Success(new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = user.RoleId,
                Username = user.Username
            });
        }, "get user by id");
    }

    public async Task<ServiceResult<UserDto>> GetUserBySessionTokenAsync(string sessionToken)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _authRepository.GetUserBySessionTokenAsync(sessionToken);
            if (user == null)
                return ServiceResult<UserDto>.NotFound("Invalid or expired session");

            return ServiceResult<UserDto>.Success(new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = user.RoleId,
                Username = user.Username
            });
        }, "get user by session token");
    }

    public async Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null || !user.IsActive)
                return ServiceResult.Failure("User not found");
            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
                return ServiceResult.Failure("Old password is incorrect");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            var updated = await _authRepository.UpdatePasswordAsync(userId, passwordHash);

            if (updated)
            {
                await _activityLogService.LogAsync("User", userId, "PASSWORD_CHANGE", $"Password changed for user '{user.Username}'", user.Username);
            }

            return !updated
                ? ServiceResult.Failure("Failed to change password")
                : ServiceResult.Success("Password changed successfully");
        }, "change password");
    }

    public async Task<ServiceResult> ValidateSessionAsync(string sessionToken)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var isValid = await _authRepository.ValidateSessionAsync(sessionToken);
            return !isValid
                ? ServiceResult.Failure("Invalid or expired session")
                : ServiceResult.Success("Session valid");
        }, "validate session");
    }
}