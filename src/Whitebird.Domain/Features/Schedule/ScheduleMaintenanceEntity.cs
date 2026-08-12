using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Schedule;

/// <summary>
/// Entity for ScheduleMaintenance table
/// </summary>
public class ScheduleMaintenanceEntity : AuditableEntity
{
    public int ScheduleMaintenanceId { get; set; }
    public DateTime ScheduleDate { get; set; }
    public int AssetId { get; set; }
    public int MaintenanceType { get; set; }
    public int? MaintenanceAssetTransactionId { get; set; }
    public int? PostAssetTransactionId { get; set; }
}