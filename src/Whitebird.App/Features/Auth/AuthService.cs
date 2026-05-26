using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.FileAttachment;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Auth;

namespace Whitebird.App.Features.Auth;

public class AuthService : BaseService, IAuthService
{
    private readonly IAuthReps _authRepository;
    private readonly IEmailService _emailService;
    private readonly IActivityLogService _activityLogService;
    private readonly IStorageService _storageService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(
        IAuthReps authRepository,
        IEmailService emailService,
        IActivityLogService activityLogService,
        IStorageService storageService,
        IHttpContextAccessor httpContextAccessor,
        ICurrentUserService currentUserService,
        ILogger<AuthService> logger) : base(logger)
    {
        _authRepository = authRepository;
        _emailService = emailService;
        _activityLogService = activityLogService;
        _storageService = storageService;
        _httpContextAccessor = httpContextAccessor;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _authRepository.GetUserByUsernameAsync(request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                await _activityLogService.LogAsync("User", 0, "LOGIN_FAILED", $"Failed login attempt for username: {request.Username}", "System");
                return ServiceResult<LoginResponse>.Failure("Invalid username or password");
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

            var requestContext = _httpContextAccessor.HttpContext?.Request;
            var profilePhotoUrl = !string.IsNullOrEmpty(user.ProfilePhotoPath) && requestContext != null
                ? $"{requestContext.Scheme}://{requestContext.Host}/api/Auth/profile-photo/{user.UserId}"
                : null;

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
                    ProfilePhotoUrl = profilePhotoUrl
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

            var request = _httpContextAccessor.HttpContext?.Request;
            var profilePhotoUrl = !string.IsNullOrEmpty(user.ProfilePhotoPath) && request != null
                ? $"{request.Scheme}://{request.Host}/api/Auth/profile-photo/{userId}"
                : null;

            return ServiceResult<UserDto>.Success(new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = user.RoleId,
                Username = user.Username,
                ProfilePhotoUrl = profilePhotoUrl
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

            var request = _httpContextAccessor.HttpContext?.Request;
            var profilePhotoUrl = !string.IsNullOrEmpty(user.ProfilePhotoPath) && request != null
                ? $"{request.Scheme}://{request.Host}/api/Auth/profile-photo/{user.UserId}"
                : null;

            return ServiceResult<UserDto>.Success(new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = user.RoleId,
                Username = user.Username,
                ProfilePhotoUrl = profilePhotoUrl
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

    public async Task<ServiceResult<string>> UploadProfilePhotoAsync(int userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return ServiceResult<string>.BadRequest("No file provided");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        if (!allowedExtensions.Contains(extension))
            return ServiceResult<string>.BadRequest($"File type '{extension}' is not allowed. Allowed: {string.Join(", ", allowedExtensions)}");

        var maxSize = 5 * 1024 * 1024;
        if (file.Length > maxSize)
            return ServiceResult<string>.BadRequest($"File size exceeds 5MB limit");

        var user = await _authRepository.GetUserByIdAsync(userId);
        if (user == null)
            return ServiceResult<string>.NotFound("User not found");

        return await ExecuteWithTransactionAsync(async () =>
        {
            if (!string.IsNullOrEmpty(user.ProfilePhotoPath))
            {
                await _storageService.DeleteFileAsync(user.ProfilePhotoPath);
            }

            var subDirectory = Path.Combine("Users", userId.ToString(), "Profile");
            var savedPath = await _storageService.SaveFileAsync(file, subDirectory);

            user.ProfilePhotoPath = savedPath;
            user.ProfilePhotoFileName = file.FileName;
            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = user.Username;

            await _authRepository.UpdateUserAsync(user);

            var request = _httpContextAccessor.HttpContext?.Request;
            var photoUrl = request != null ? $"{request.Scheme}://{request.Host}/api/Auth/profile-photo/{userId}" : null;

            await _activityLogService.LogUpdateAsync(
                "User",
                userId,
                $"Profile photo uploaded for user '{user.Username}'",
                user.Username);

            return ServiceResult<string>.Success(photoUrl ?? string.Empty, "Profile photo uploaded successfully");
        }, "upload profile photo");
    }

    public async Task<ServiceResult<byte[]>> GetProfilePhotoAsync(int userId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null)
                return ServiceResult<byte[]>.NotFound("User not found");

            if (string.IsNullOrEmpty(user.ProfilePhotoPath))
                return ServiceResult<byte[]>.NotFound("Profile photo not found");

            var fileBytes = await _storageService.ReadFileAsync(user.ProfilePhotoPath);
            return ServiceResult<byte[]>.Success(fileBytes);
        }, "get profile photo");
    }

    public async Task<ServiceResult> DeleteProfilePhotoAsync(int userId)
    {
        var user = await _authRepository.GetUserByIdAsync(userId);
        if (user == null)
            return ServiceResult.NotFound("User not found");

        return await ExecuteWithTransactionAsync(async () =>
        {
            if (string.IsNullOrEmpty(user.ProfilePhotoPath))
                return ServiceResult.Success("No profile photo to delete");

            await _storageService.DeleteFileAsync(user.ProfilePhotoPath);

            user.ProfilePhotoPath = null;
            user.ProfilePhotoFileName = null;
            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = user.Username;

            await _authRepository.UpdateUserAsync(user);

            await _activityLogService.LogUpdateAsync(
                "User",
                userId,
                $"Profile photo deleted for user '{user.Username}'",
                user.Username);

            return ServiceResult.Success("Profile photo deleted successfully");
        }, "delete profile photo");
    }

    // ========== NEW METHOD ==========
    public async Task<ServiceResult<UserDto>> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if (user == null || !user.IsActive)
                return ServiceResult<UserDto>.NotFound("User not found");

            var oldFullName = user.FullName;
            var oldEmail = user.Email;
            var oldPhoneNumber = user.PhoneNumber;

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = user.Username;

            var result = await _authRepository.UpdateUserAsync(user);
            if (result <= 0)
                return ServiceResult<UserDto>.Failure("Failed to update profile");

            await _activityLogService.LogUpdateAsync(
                "User",
                userId,
                $"Profile updated: Name '{oldFullName}' -> '{request.FullName}', Email '{oldEmail}' -> '{request.Email}', Phone '{oldPhoneNumber}' -> '{request.PhoneNumber}'",
                user.Username);

            var requestContext = _httpContextAccessor.HttpContext?.Request;
            var profilePhotoUrl = !string.IsNullOrEmpty(user.ProfilePhotoPath) && requestContext != null
                ? $"{requestContext.Scheme}://{requestContext.Host}/api/Auth/profile-photo/{userId}"
                : null;

            return ServiceResult<UserDto>.Success(new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = user.RoleId,
                Username = user.Username,
                ProfilePhotoUrl = profilePhotoUrl
            }, "Profile updated successfully");
        }, "update profile", async (ex) =>
        {
            await _activityLogService.LogErrorAsync("User", userId, "Update Profile", ex, _currentUserService.GetDisplayName());
        });
    }
}