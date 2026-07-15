using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Schedule;

/// <summary>
/// Grid request for Schedule
/// </summary>
public class ScheduleGridRequest : GridRequestBase
{
    /// <summary>
    /// Filter by AssetId
    /// </summary>
    public int? AssetId { get; set; }

    /// <summary>
    /// Filter by EmployeeId (for Handover/Return)
    /// </summary>
    public int? EmployeeId { get; set; }

    /// <summary>
    /// Filter by MaintenanceType (for ScheduleMaintenance)
    /// </summary>
    public int? MaintenanceType { get; set; }

    /// <summary>
    /// Filter by date range - start date
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// Filter by date range - end date
    /// </summary>
    public DateTime? DateTo { get; set; }
}