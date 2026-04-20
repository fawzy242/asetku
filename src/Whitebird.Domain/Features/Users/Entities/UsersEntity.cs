using Whitebird.Domain.Features.Common.Entities;

namespace Whitebird.Domain.Features.Users.Entities;

public class UsersEntity : AuditableEntity
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? RoleId { get; set; }
    public string? SessionToken { get; set; }
    public DateTime? SessionExpiry { get; set; }
    public bool IsLocked { get; set; }
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }
    public DateTime? LastLoginDate { get; set; }
}