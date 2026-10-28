using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Roles;

public class RolesEntity : AuditableEntity
{
    public int RoleId { get; set; }
    public string? RoleCode { get; set; }
    public string RoleName { get; set; } = default!;
    public string? Description { get; set; }
}