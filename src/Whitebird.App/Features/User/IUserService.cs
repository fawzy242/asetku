using Microsoft.AspNetCore.Http;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Common;

namespace Whitebird.App.Features.User;

/// <summary>
/// Service interface for User business logic (Profile Management)
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets user by session token
    /// </summary>
    Task<ServiceResult<UserDto>> GetUserBySessionTokenAsync(string sessionToken);

    /// <summary>
    /// Gets user by ID
    /// </summary>
    Task<ServiceResult<UserDto>> GetUserByIdAsync(int userId);

    /// <summary>
    /// Updates user profile (FullName, Email, PhoneNumber)
    /// </summary>
    Task<ServiceResult<UserDto>> UpdateProfileAsync(int userId, UpdateProfileRequest request);

    /// <summary>
    /// Changes user password
    /// </summary>
    Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordRequest request);

    /// <summary>
    /// Uploads profile photo
    /// </summary>
    Task<ServiceResult<string>> UploadProfilePhotoAsync(int userId, IFormFile file);

    /// <summary>
    /// Gets profile photo bytes
    /// </summary>
    Task<ServiceResult<byte[]>> GetProfilePhotoAsync(int userId);

    /// <summary>
    /// Deletes profile photo
    /// </summary>
    Task<ServiceResult> DeleteProfilePhotoAsync(int userId);
}