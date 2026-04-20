using Dapper;
using Whitebird.Infra.Database;
using Whitebird.Domain.Features.Users.Entities;

namespace Whitebird.Infra.Features.Auth;

public class AuthReps : IAuthReps
{
    private readonly DapperContext _context;

    public AuthReps(DapperContext context)
    {
        _context = context;
    }

    public async Task<UsersEntity?> GetUserByEmailAsync(string email)
    {
        const string sql = "SELECT * FROM Users WHERE Email = @Email AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<UsersEntity>(sql, new { Email = email });
    }

    public async Task<UsersEntity?> GetUserBySessionTokenAsync(string sessionToken)
    {
        const string sql = @"
            SELECT * FROM Users 
            WHERE SessionToken = @SessionToken AND SessionExpiry > GETDATE() AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<UsersEntity>(sql, new { SessionToken = sessionToken });
    }

    public async Task<UsersEntity?> GetUserByResetTokenAsync(string email, string resetToken)
    {
        const string sql = @"
            SELECT * FROM Users 
            WHERE Email = @Email AND ResetToken = @ResetToken AND ResetTokenExpiry > GETDATE() AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<UsersEntity>(sql, new { Email = email, ResetToken = resetToken });
    }

    public async Task<UsersEntity?> GetUserByIdAsync(int userId)
    {
        const string sql = "SELECT * FROM Users WHERE UserId = @UserId AND IsActive = 1";
        return await _context.QueryFirstOrDefaultAsync<UsersEntity>(sql, new { UserId = userId });
    }

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { Email = email }) > 0;
    }

    public async Task<bool> UpdateResetTokenAsync(int userId, string resetToken, DateTime expiry)
    {
        const string sql = @"
            UPDATE Users SET ResetToken = @ResetToken, ResetTokenExpiry = @Expiry, ModifiedDate = GETDATE()
            WHERE UserId = @UserId AND IsActive = 1";
        return await _context.ExecuteAsync(sql, new { UserId = userId, ResetToken = resetToken, Expiry = expiry }) > 0;
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string passwordHash)
    {
        const string sql = @"
            UPDATE Users SET PasswordHash = @PasswordHash, ResetToken = NULL, ResetTokenExpiry = NULL, ModifiedDate = GETDATE()
            WHERE UserId = @UserId AND IsActive = 1";
        return await _context.ExecuteAsync(sql, new { UserId = userId, PasswordHash = passwordHash }) > 0;
    }

    public async Task<bool> ClearResetTokenAsync(int userId)
    {
        const string sql = "UPDATE Users SET ResetToken = NULL, ResetTokenExpiry = NULL, ModifiedDate = GETDATE() WHERE UserId = @UserId";
        return await _context.ExecuteAsync(sql, new { UserId = userId }) > 0;
    }

    public async Task<bool> UpdateLastLoginAsync(int userId)
    {
        const string sql = "UPDATE Users SET LastLoginDate = GETDATE(), ModifiedDate = GETDATE() WHERE UserId = @UserId AND IsActive = 1";
        return await _context.ExecuteAsync(sql, new { UserId = userId }) > 0;
    }

    public async Task<bool> CreateSessionAsync(int userId, string sessionToken, DateTime expiry)
    {
        const string sql = @"
            UPDATE Users SET SessionToken = @SessionToken, SessionExpiry = @Expiry, ModifiedDate = GETDATE()
            WHERE UserId = @UserId AND IsActive = 1";
        return await _context.ExecuteAsync(sql, new { UserId = userId, SessionToken = sessionToken, Expiry = expiry }) > 0;
    }

    public async Task<bool> ClearSessionAsync(int userId)
    {
        const string sql = "UPDATE Users SET SessionToken = NULL, SessionExpiry = NULL, ModifiedDate = GETDATE() WHERE UserId = @UserId";
        return await _context.ExecuteAsync(sql, new { UserId = userId }) > 0;
    }

    public async Task<bool> ClearSessionByTokenAsync(string sessionToken)
    {
        const string sql = "UPDATE Users SET SessionToken = NULL, SessionExpiry = NULL, ModifiedDate = GETDATE() WHERE SessionToken = @SessionToken";
        return await _context.ExecuteAsync(sql, new { SessionToken = sessionToken }) > 0;
    }

    public async Task<bool> ValidateSessionAsync(string sessionToken)
    {
        const string sql = "SELECT COUNT(1) FROM Users WHERE SessionToken = @SessionToken AND SessionExpiry > GETDATE() AND IsActive = 1";
        return await _context.ExecuteScalarAsync<int>(sql, new { SessionToken = sessionToken }) > 0;
    }
}