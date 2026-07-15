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