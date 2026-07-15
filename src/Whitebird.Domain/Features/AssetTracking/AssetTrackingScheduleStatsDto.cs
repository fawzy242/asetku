namespace Whitebird.Domain.Features.AssetTracking;

/// <summary>
/// Schedule stats for a specific asset (grid data)
/// </summary>
public class AssetTrackingScheduleStatsDto
{
    public int ScheduleId { get; set; }
    public DateTime ScheduleDate { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? EmployeeName { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }
    public int? RelatedTransactionId { get; set; }
    public bool IsActive { get; set; }
}