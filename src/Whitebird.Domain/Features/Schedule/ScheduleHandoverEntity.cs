using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Schedule;

public class ScheduleHandoverEntity : AuditableEntity
{
    public int ScheduleHandoverId { get; set; }
    public DateTime ScheduleDate { get; set; }
    public int AssetId { get; set; }
    public int EmployeeId { get; set; }
    public int? HandoverAssetTransactionId { get; set; }
}