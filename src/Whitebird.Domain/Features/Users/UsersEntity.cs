using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Users;

public class UsersEntity : AuditableEntity
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    // NOTE: Role is NOT a column on Users. Roles live in Roles/UserRoles (many-to-many).
    // Resolution is done via IUserReps.GetUserRoleAsync(userId); see AuthService and UserService.
    public string? SessionToken { get; set; }
    public DateTime? SessionExpiry { get; set; }
    public bool IsLocked { get; set; }
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public bool MustChangePassword { get; set; }
}