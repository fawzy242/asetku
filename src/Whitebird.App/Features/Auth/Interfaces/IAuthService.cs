using Whitebird.App.Features.Common.Service;
using Whitebird.Domain.Features.Common.Auth;

namespace Whitebird.App.Features.Auth.Interfaces;

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
}