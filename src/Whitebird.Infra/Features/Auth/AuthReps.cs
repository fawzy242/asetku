using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Users;

namespace Whitebird.Infra.Features.Auth;

/// <summary>
/// Repository implementation for Authentication operations using Dapper
/// </summary>
public class AuthReps : IAuthReps
{
    private readonly DapperContext _context;

    public AuthReps(DapperContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<UsersEntity?> GetUserByEmailAsync(string email)
    {
        const string sql = "SELECT * FROM Users WHERE Email = @Email AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<UsersEntity>(sql, new { Email = email });
    }

    /// <inheritdoc />
    public async Task<UsersEntity?> GetUserByUsernameAsync(string username)
    {
        const string sql = "SELECT * FROM Users WHERE Username = @Username AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<UsersEntity>(sql, new { Username = username });
    }

    /// <inheritdoc />
    public async Task<UsersEntity?> GetUserBySessionTokenAsync(string sessionToken)
    {
        const string sql = @"
            SELECT * FROM Users 
            WHERE SessionToken = @SessionToken AND SessionExpiry > GETDATE() AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<UsersEntity>(sql, new { SessionToken = sessionToken });
    }

    /// <inheritdoc />
    public async Task<UsersEntity?> GetUserByResetTokenAsync(string email, string resetToken)
    {
        const string sql = @"
            SELECT * FROM Users 
            WHERE Email = @Email AND ResetToken = @ResetToken AND ResetTokenExpiry > GETDATE() AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<UsersEntity>(sql, new { Email = email, ResetToken = resetToken });
    }

    /// <inheritdoc />
    public async Task<UsersEntity?> GetUserByIdAsync(int userId)
    {
        const string sql = "SELECT * FROM Users WHERE UserId = @UserId AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<UsersEntity>(sql, new { UserId = userId });
    }

    /// <inheritdoc />
    public async Task<bool> IsEmailExistsAsync(string email)
    {
        const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { Email = email }) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateResetTokenAsync(int userId, string resetToken, DateTime expiry)
    {
        const string sql = @"
            UPDATE Users SET ResetToken = @ResetToken, ResetTokenExpiry = @Expiry, ModifiedDate = GETDATE()
            WHERE UserId = @UserId AND IsActive = 1";
        return await _context.ExecuteAsync(sql, new { UserId = userId, ResetToken = resetToken, Expiry = expiry }) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> UpdatePasswordAsync(int userId, string passwordHash)
    {
        const string sql = @"
            UPDATE Users SET PasswordHash = @PasswordHash, ResetToken = NULL, ResetTokenExpiry = NULL, ModifiedDate = GETDATE()
            WHERE UserId = @UserId AND IsActive = 1";
        return await _context.ExecuteAsync(sql, new { UserId = userId, PasswordHash = passwordHash }) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> ClearResetTokenAsync(int userId)
    {
        const string sql = "UPDATE Users SET ResetToken = NULL, ResetTokenExpiry = NULL, ModifiedDate = GETDATE() WHERE UserId = @UserId";
        return await _context.ExecuteAsync(sql, new { UserId = userId }) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateLastLoginAsync(int userId)
    {
        const string sql = "UPDATE Users SET LastLoginDate = GETDATE(), ModifiedDate = GETDATE() WHERE UserId = @UserId AND IsActive = 1";
        return await _context.ExecuteAsync(sql, new { UserId = userId }) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> CreateSessionAsync(int userId, string sessionToken, DateTime expiry)
    {
        const string sql = @"
            UPDATE Users SET SessionToken = @SessionToken, SessionExpiry = @Expiry, ModifiedDate = GETDATE()
            WHERE UserId = @UserId AND IsActive = 1";
        return await _context.ExecuteAsync(sql, new { UserId = userId, SessionToken = sessionToken, Expiry = expiry }) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> ClearSessionAsync(int userId)
    {
        const string sql = "UPDATE Users SET SessionToken = NULL, SessionExpiry = NULL, ModifiedDate = GETDATE() WHERE UserId = @UserId";
        return await _context.ExecuteAsync(sql, new { UserId = userId }) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> ClearSessionByTokenAsync(string sessionToken)
    {
        const string sql = "UPDATE Users SET SessionToken = NULL, SessionExpiry = NULL, ModifiedDate = GETDATE() WHERE SessionToken = @SessionToken";
        return await _context.ExecuteAsync(sql, new { SessionToken = sessionToken }) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> ValidateSessionAsync(string sessionToken)
    {
        const string sql = "SELECT COUNT(1) FROM Users WHERE SessionToken = @SessionToken AND SessionExpiry > GETDATE() AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { SessionToken = sessionToken }) > 0;
    }

    /// <inheritdoc />
    public async Task<int> UpdateUserAsync(UsersEntity user)
    {
        const string sql = @"
            UPDATE Users SET 
                FullName = @FullName,
                Email = @Email,
                PhoneNumber = @PhoneNumber,
                ProfilePhotoPath = @ProfilePhotoPath,
                ProfilePhotoFileName = @ProfilePhotoFileName,
                ModifiedDate = @ModifiedDate,
                ModifiedBy = @ModifiedBy
            WHERE UserId = @UserId AND IsActive = 1";

        return await _context.ExecuteAsync(sql, user);
    }
}