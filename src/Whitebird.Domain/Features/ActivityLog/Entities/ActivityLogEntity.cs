using Whitebird.Domain.Features.Common.Entities;

namespace Whitebird.Domain.Features.ActivityLog.Entities;

public class ActivityLogEntity : AuditableEntity
{
    public int LogId { get; set; }
    public string ReferenceTable { get; set; } = default!;
    public int ReferenceId { get; set; }
    public string ActivityType { get; set; } = default!;
    public string? Description { get; set; }
}