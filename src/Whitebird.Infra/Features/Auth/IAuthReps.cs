using Whitebird.Domain.Features.Users;

namespace Whitebird.Infra.Features.Auth;

public interface IAuthReps
{
    // Authentication
    Task<UsersEntity?> GetUserByEmailAsync(string email);
    Task<UsersEntity?> GetUserByUsernameAsync(string username);
    Task<UsersEntity?> GetUserByResetTokenAsync(string email, string resetToken);
    Task<bool> IsEmailExistsAsync(string email);
    
    // Session Management
    Task<bool> CreateSessionAsync(int userId, string sessionToken, DateTime expiry);
    Task<bool> ClearSessionAsync(int userId);
    Task<bool> ClearSessionByTokenAsync(string sessionToken);
    Task<bool> ValidateSessionAsync(string sessionToken);
    
    // Password Management
    Task<bool> UpdateResetTokenAsync(int userId, string resetToken, DateTime expiry);
    Task<bool> UpdatePasswordAsync(int userId, string passwordHash);
    Task<bool> ClearResetTokenAsync(int userId);
    
    // Login Tracking
    Task<bool> UpdateLastLoginAsync(int userId);
}