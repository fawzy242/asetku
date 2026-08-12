using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Auth;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.FileAttachment;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.Users;
using Whitebird.Infra.Features.Auth;
using Whitebird.Infra.Features.User;

namespace Whitebird.App.Features.User;

/// <summary>
/// Service implementation for User business logic (Profile Management)
/// </summary>
public class UserService : BaseService, IUserService
{
    private readonly IUserReps _userReps;
    private readonly IAuthReps _authReps;
    private readonly IStorageService _storageService;
    private readonly IActivityLogService _activityLogService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(
        IUserReps userReps,
        IAuthReps authReps,
        IStorageService storageService,
        IActivityLogService activityLogService,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserService> logger) : base(logger)
    {
        _userReps = userReps;
        _authReps = authReps;
        _storageService = storageService;
        _activityLogService = activityLogService;
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ServiceResult<UserDto>> GetUserBySessionTokenAsync(string sessionToken)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _userReps.GetUserBySessionTokenAsync(sessionToken);
            if (user == null)
            {
                return ServiceResult<UserDto>.NotFound("Invalid or expired session");
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            var profilePhotoUrl = !string.IsNullOrEmpty(user.ProfilePhotoPath) && request != null
                ? $"{request.Scheme}://{request.Host}/api/User/profile-photo"
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

    public async Task<ServiceResult<UserDto>> GetUserByIdAsync(int userId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _userReps.GetUserByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return ServiceResult<UserDto>.NotFound("User not found");
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            var profilePhotoUrl = !string.IsNullOrEmpty(user.ProfilePhotoPath) && request != null
                ? $"{request.Scheme}://{request.Host}/api/User/profile-photo"
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

    public async Task<ServiceResult<UserDto>> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var user = await _userReps.GetUserByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return ServiceResult<UserDto>.NotFound("User not found");
            }

            var oldFullName = user.FullName;
            var oldEmail = user.Email;
            var oldPhoneNumber = user.PhoneNumber;

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _userReps.UpdateUserAsync(user);
            if (result <= 0)
            {
                return ServiceResult<UserDto>.Failure("Failed to update profile");
            }

            await _activityLogService.LogUpdateAsync(
                "Users",
                userId,
                $"Profile updated: Name '{oldFullName}' -> '{request.FullName}', Email '{oldEmail}' -> '{request.Email}', Phone '{oldPhoneNumber}' -> '{request.PhoneNumber}'",
                _currentUserService.GetDisplayName());

            var requestContext = _httpContextAccessor.HttpContext?.Request;
            var profilePhotoUrl = !string.IsNullOrEmpty(user.ProfilePhotoPath) && requestContext != null
                ? $"{requestContext.Scheme}://{requestContext.Host}/api/User/profile-photo"
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
        }, "update profile");
    }

    public async Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _userReps.GetUserByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return ServiceResult.Failure("User not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
            {
                return ServiceResult.Failure("Old password is incorrect");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            var updated = await _authReps.UpdatePasswordAsync(userId, passwordHash);

            if (updated)
            {
                await _activityLogService.LogUpdateAsync(
                    "Users",
                    userId,
                    $"Password changed for user '{user.Username}'",
                    _currentUserService.GetDisplayName());
            }

            return !updated
                ? ServiceResult.Failure("Failed to change password")
                : ServiceResult.Success("Password changed successfully");
        }, "change password");
    }

    public async Task<ServiceResult<string>> UploadProfilePhotoAsync(int userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return ServiceResult<string>.BadRequest("No file provided");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!FileSizeConstants.IsAllowedProfilePhotoExtension(extension))
        {
            return ServiceResult<string>.BadRequest($"File type '{extension}' is not allowed. Allowed: {string.Join(", ", FileSizeConstants.AllowedProfilePhotoExtensions)}");
        }

        if (file.Length > FileSizeConstants.MaxProfilePhotoBytes)
        {
            return ServiceResult<string>.BadRequest($"File size exceeds {FileSizeConstants.MaxProfilePhotoMB}MB limit");
        }

        var user = await _userReps.GetUserByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult<string>.NotFound("User not found");
        }

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
            user.ModifiedBy = _currentUserService.GetDisplayName();

            await _userReps.UpdateUserAsync(user);

            var request = _httpContextAccessor.HttpContext?.Request;
            var photoUrl = request != null ? $"{request.Scheme}://{request.Host}/api/User/profile-photo" : null;

            await _activityLogService.LogUpdateAsync(
                "Users",
                userId,
                $"Profile photo uploaded for user '{user.Username}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<string>.Success(photoUrl ?? string.Empty, "Profile photo uploaded successfully");
        }, "upload profile photo");
    }

    public async Task<ServiceResult<byte[]>> GetProfilePhotoAsync(int userId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _userReps.GetUserByIdAsync(userId);
            if (user == null)
            {
                return ServiceResult<byte[]>.NotFound("User not found");
            }

            if (string.IsNullOrEmpty(user.ProfilePhotoPath))
            {
                return ServiceResult<byte[]>.NotFound("Profile photo not found");
            }

            var fileBytes = await _storageService.ReadFileAsync(user.ProfilePhotoPath);
            return ServiceResult<byte[]>.Success(fileBytes);
        }, "get profile photo");
    }

    public async Task<ServiceResult> DeleteProfilePhotoAsync(int userId)
    {
        var user = await _userReps.GetUserByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult.NotFound("User not found");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            if (string.IsNullOrEmpty(user.ProfilePhotoPath))
            {
                return ServiceResult.Success("No profile photo to delete");
            }

            await _storageService.DeleteFileAsync(user.ProfilePhotoPath);

            user.ProfilePhotoPath = null;
            user.ProfilePhotoFileName = null;
            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = _currentUserService.GetDisplayName();

            await _userReps.UpdateUserAsync(user);

            await _activityLogService.LogUpdateAsync(
                "Users",
                userId,
                $"Profile photo deleted for user '{user.Username}'",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Profile photo deleted successfully");
        }, "delete profile photo");
    }
}