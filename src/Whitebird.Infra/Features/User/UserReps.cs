using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Users;

namespace Whitebird.Infra.Features.User;

/// <summary>
/// Repository implementation for User operations using Dapper
/// </summary>
public class UserReps : IUserReps
{
    private readonly DapperContext _context;

    public UserReps(DapperContext context)
    {
        _context = context;
    }

    // ============================================================
    // USER
    // ============================================================

    public async Task<UsersEntity?> GetUserByIdAsync(int userId)
    {
        const string sql = "SELECT * FROM Users WHERE UserId = @UserId AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<UsersEntity>(sql, new { UserId = userId });
    }

    public async Task<UsersEntity?> GetUserBySessionTokenAsync(string sessionToken)
    {
        const string sql = @"
            SELECT * FROM Users 
            WHERE SessionToken = @SessionToken AND SessionExpiry > GETDATE() AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<UsersEntity>(sql, new { SessionToken = sessionToken });
    }

    public async Task<int> UpdateUserAsync(UsersEntity user)
    {
        // FIX: Hanya update field yang ada di database
        const string sql = @"
            UPDATE Users SET 
                FullName = @FullName,
                Email = @Email,
                PhoneNumber = @PhoneNumber,
                ModifiedDate = @ModifiedDate,
                ModifiedBy = @ModifiedBy
            WHERE UserId = @UserId AND IsActive = 1";

        return await _context.ExecuteAsync(sql, user);
    }

    // ============================================================
    // USER PHOTO PROFILE - FIXED
    // ============================================================

    /// <summary>
    /// Insert user photo profile attachment
    /// </summary>
    public async Task<int> InsertPhotoProfileAsync(UsersPhotoProfileAttachmentEntity entity)
    {
        const string sql = @"
            INSERT INTO UsersPhotoProfileAttachment (
                UserId, FileAttachmentId, IsActive, CreatedDate, CreatedBy
            ) VALUES (
                @UserId, @FileAttachmentId, 1, GETDATE(), @CreatedBy
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        return await _context.ExecuteScalarAsync<int>(sql, entity);
    }

    /// <summary>
    /// Get user photo profile by user ID
    /// </summary>
    public async Task<UsersPhotoProfileAttachmentEntity?> GetPhotoProfileByUserIdAsync(int userId)
    {
        const string sql = @"
            SELECT * FROM UsersPhotoProfileAttachment 
            WHERE UserId = @UserId AND IsActive = 1";

        return await _context.QueryFirstOrDefaultAsync<UsersPhotoProfileAttachmentEntity>(sql, new { UserId = userId });
    }

    /// <summary>
    /// Delete user photo profile by user ID (soft delete)
    /// </summary>
    public async Task<int> DeletePhotoProfileByUserIdAsync(int userId)
    {
        const string sql = @"
            UPDATE UsersPhotoProfileAttachment 
            SET IsActive = 0, ModifiedDate = GETDATE() 
            WHERE UserId = @UserId AND IsActive = 1";

        return await _context.ExecuteAsync(sql, new { UserId = userId });
    }

    /// <summary>
    /// Delete user photo profile by attachment ID (soft delete)
    /// </summary>
    public async Task<int> DeletePhotoProfileByFileAttachmentIdAsync(int fileAttachmentId)
    {
        const string sql = @"
            UPDATE UsersPhotoProfileAttachment 
            SET IsActive = 0, ModifiedDate = GETDATE() 
            WHERE FileAttachmentId = @FileAttachmentId AND IsActive = 1";

        return await _context.ExecuteAsync(sql, new { FileAttachmentId = fileAttachmentId });
    }
}