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
/// Service implementation for User business logic (Profile Management).
///
/// Profile photo operations are routed through the internal
/// <see cref="IFileAttachmentService"/>, matching the pattern used by
/// Asset, Employee, and AssetTransaction. The service is the shared
/// file-storage primitive; it is not exposed as its own API module.
/// </summary>
public class UserService : BaseService, IUserService
{
    private readonly IUserReps _userReps;
    private readonly IAuthReps _authReps;
    private readonly IFileAttachmentService _fileAttachmentService;
    private readonly IActivityLogService _activityLogService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(
        IUserReps userReps,
        IAuthReps authReps,
        IFileAttachmentService fileAttachmentService,
        IActivityLogService activityLogService,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserService> logger) : base(logger)
    {
        _userReps = userReps;
        _authReps = authReps;
        _fileAttachmentService = fileAttachmentService;
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

            var (roleId, roleName) = await _userReps.GetUserRoleAsync(user.UserId);

            var request = _httpContextAccessor.HttpContext?.Request;
            var profilePhotoUrl = request != null
                ? $"{request.Scheme}://{request.Host}/api/User/profile-photo"
                : null;

            return ServiceResult<UserDto>.Success(new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = roleId,
                RoleName = roleName,
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

            var (roleId, roleName) = await _userReps.GetUserRoleAsync(user.UserId);

            var request = _httpContextAccessor.HttpContext?.Request;
            var profilePhotoUrl = request != null
                ? $"{request.Scheme}://{request.Host}/api/User/profile-photo"
                : null;

            return ServiceResult<UserDto>.Success(new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = roleId,
                RoleName = roleName,
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
                TableNames.Users,
                userId,
                $"Profile updated: Name '{oldFullName}' -> '{request.FullName}', Email '{oldEmail}' -> '{request.Email}', Phone '{oldPhoneNumber}' -> '{request.PhoneNumber}'",
                _currentUserService.GetDisplayName());

            var (roleId, roleName) = await _userReps.GetUserRoleAsync(user.UserId);

            var requestContext = _httpContextAccessor.HttpContext?.Request;
            var profilePhotoUrl = requestContext != null
                ? $"{requestContext.Scheme}://{requestContext.Host}/api/User/profile-photo"
                : null;

            return ServiceResult<UserDto>.Success(new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = roleId,
                RoleName = roleName,
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
                    TableNames.Users,
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
        // Centralised upload validation (extension + size).
        var (isValid, error) = FileUploadValidator.ValidateProfilePhoto(file);
        if (!isValid)
        {
            return ServiceResult<string>.BadRequest(error!);
        }

        var user = await _userReps.GetUserByIdAsync(userId);
        if (user == null)
        {
            return ServiceResult<string>.NotFound("User not found");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            // Replace any existing photo. Metadata soft-delete + (Batch 3) physical
            // file removal is handled by IFileAttachmentService.DeleteAsync.
            var existingPhoto = await _userReps.GetPhotoProfileByUserIdAsync(userId);
            if (existingPhoto != null)
            {
                await _userReps.DeletePhotoProfileByUserIdAsync(userId);
                await _fileAttachmentService.DeleteAsync(existingPhoto.FileAttachmentId);
            }

            // Read the uploaded stream into bytes for the shared upload path.
            using var stream = file!.OpenReadStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            // Upload through the shared internal file-attachment service. This is
            // the same primitive used by Asset/Employee/AssetTransaction; it
            // performs the storage-layer defence-in-depth extension/size check and
            // builds a FileUrl pointing at /api/User/profile-photo.
            var uploadResult = await _fileAttachmentService.UploadBytesAsync(
                referenceTable: "UserPhotoProfile",
                referenceId: userId,
                originalFileName: file.FileName,
                bytes: bytes,
                fileCategory: "ProfilePhoto",
                description: $"Profile photo for user {user.Username}",
                isPrimary: true);

            if (!uploadResult.IsSuccess || uploadResult.Data == null)
            {
                return ServiceResult<string>.Failure(uploadResult.Errors);
            }

            // Link the stored file to the user via the join table.
            var photoProfileEntity = new UsersPhotoProfileAttachmentEntity
            {
                UserId = userId,
                FileAttachmentId = uploadResult.Data.FileAttachmentId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = _currentUserService.GetDisplayName()
            };

            await _userReps.InsertPhotoProfileAsync(photoProfileEntity);

            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = _currentUserService.GetDisplayName();
            await _userReps.UpdateUserAsync(user);

            await _activityLogService.LogUpdateAsync(
                TableNames.Users,
                userId,
                $"Profile photo uploaded for user '{user.Username}'",
                _currentUserService.GetDisplayName());

            var photoUrl = uploadResult.Data.FileUrl ?? string.Empty;
            return ServiceResult<string>.Success(photoUrl, "Profile photo uploaded successfully");
        }, "upload profile photo");
    }

    public async Task<ServiceResult<FileContentDto>> GetProfilePhotoAsync(int userId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var user = await _userReps.GetUserByIdAsync(userId);
            if (user == null)
            {
                return ServiceResult<FileContentDto>.NotFound("User not found");
            }

            var photoProfile = await _userReps.GetPhotoProfileByUserIdAsync(userId);
            if (photoProfile == null)
            {
                return ServiceResult<FileContentDto>.NotFound("Profile photo not found");
            }

            // Shared primitive: returns bytes + content-type + file-name in one call.
            return await _fileAttachmentService.DownloadAsync(photoProfile.FileAttachmentId);
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
            var photoProfile = await _userReps.GetPhotoProfileByUserIdAsync(userId);
            if (photoProfile == null)
            {
                return ServiceResult.Success("No profile photo to delete");
            }

            await _userReps.DeletePhotoProfileByUserIdAsync(userId);
            await _fileAttachmentService.DeleteAsync(photoProfile.FileAttachmentId);

            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = _currentUserService.GetDisplayName();
            await _userReps.UpdateUserAsync(user);

            await _activityLogService.LogUpdateAsync(
                TableNames.Users,
                userId,
                $"Profile photo deleted for user '{user.Username}'",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Profile photo deleted successfully");
        }, "delete profile photo");
    }
}