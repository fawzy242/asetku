using Microsoft.AspNetCore.Http;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Common;

namespace Whitebird.App.Features.Auth;

public interface IAuthService
{
    Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ServiceResult> LogoutAsync(string sessionToken);
    Task<ServiceResult> ForgotPasswordAsync(string email);
    Task<ServiceResult> ResetPasswordAsync(int userId, ResetPasswordRequest request);
    Task<ServiceResult> ResetPasswordWithTokenAsync(ResetPasswordWithTokenRequest request);
    Task<ServiceResult<UserDto>> GetUserByIdAsync(int userId);
    Task<ServiceResult<UserDto>> GetUserBySessionTokenAsync(string sessionToken);
    Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<ServiceResult> ValidateSessionAsync(string sessionToken);
    Task<ServiceResult<string>> UploadProfilePhotoAsync(int userId, IFormFile file);
    Task<ServiceResult<byte[]>> GetProfilePhotoAsync(int userId);
    Task<ServiceResult> DeleteProfilePhotoAsync(int userId);

    // ========== NEW METHOD ==========
    Task<ServiceResult<UserDto>> UpdateProfileAsync(int userId, UpdateProfileRequest request);
}