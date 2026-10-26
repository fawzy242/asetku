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
    /// Updates user profile (FullName, Email, PhoneNumber)
    /// </summary>
    Task<int> UpdateUserAsync(UsersEntity user);

    /// <summary>
    /// Resolves the user's first active role via UserRoles/Roles.
    /// Returns (null, null) when the user has no assigned role.
    /// </summary>
    Task<(int? RoleId, string? RoleName)> GetUserRoleAsync(int userId);

    // ============================================================
    // USER PHOTO PROFILE
    // ============================================================

    /// <summary>
    /// Insert user photo profile attachment
    /// </summary>
    Task<int> InsertPhotoProfileAsync(UsersPhotoProfileAttachmentEntity entity);

    /// <summary>
    /// Get the most recent active user photo profile by user ID
    /// </summary>
    Task<UsersPhotoProfileAttachmentEntity?> GetPhotoProfileByUserIdAsync(int userId);

    /// <summary>
    /// Delete user photo profile by user ID (soft delete)
    /// </summary>
    Task<int> DeletePhotoProfileByUserIdAsync(int userId);

    /// <summary>
    /// Delete user photo profile by attachment ID (soft delete)
    /// </summary>
    Task<int> DeletePhotoProfileByFileAttachmentIdAsync(int fileAttachmentId);
}