using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Schedule;

/// <summary>
/// Entity for ScheduleReturn table
/// </summary>
public class ScheduleReturnEntity : AuditableEntity
{
    public int ScheduleReturnId { get; set; }
    public DateTime ScheduleDate { get; set; }
    public int AssetId { get; set; }
    public int EmployeeId { get; set; }
    public int? ReturnAssetTransactionId { get; set; }
}