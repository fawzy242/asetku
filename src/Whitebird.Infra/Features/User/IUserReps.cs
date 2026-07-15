using Whitebird.Domain.Features.Users;

namespace Whitebird.Infra.Features.User;

/// <summary>
/// Repository interface for User operations (Profile Management)
/// </summary>
public interface IUserReps
{
    /// <summary>
    /// Gets user by ID
    /// </summary>
    Task<UsersEntity?> GetUserByIdAsync(int userId);
    
    /// <summary>
    /// Gets user by session token
    /// </summary>
    Task<UsersEntity?> GetUserBySessionTokenAsync(string sessionToken);
    
    /// <summary>
    /// Updates user profile
    /// </summary>
    Task<int> UpdateUserAsync(UsersEntity user);
}