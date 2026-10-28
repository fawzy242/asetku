using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.UserRoles;

public class UserRolesEntity : AuditableEntity
{
    public int UserRoleId { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }
}