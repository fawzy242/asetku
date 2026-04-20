using Whitebird.Domain.Features.Users.Entities;

namespace Whitebird.Infra.Features.Auth;

public interface IAuthReps
{
    Task<UsersEntity?> GetUserByEmailAsync(string email);
    Task<UsersEntity?> GetUserBySessionTokenAsync(string sessionToken);
    Task<UsersEntity?> GetUserByResetTokenAsync(string email, string resetToken);
    Task<UsersEntity?> GetUserByIdAsync(int userId);
    Task<bool> IsEmailExistsAsync(string email);
    Task<bool> UpdateResetTokenAsync(int userId, string resetToken, DateTime expiry);
    Task<bool> UpdatePasswordAsync(int userId, string passwordHash);
    Task<bool> ClearResetTokenAsync(int userId);
    Task<bool> UpdateLastLoginAsync(int userId);
    Task<bool> CreateSessionAsync(int userId, string sessionToken, DateTime expiry);
    Task<bool> ClearSessionAsync(int userId);
    Task<bool> ClearSessionByTokenAsync(string sessionToken);
    Task<bool> ValidateSessionAsync(string sessionToken);
}