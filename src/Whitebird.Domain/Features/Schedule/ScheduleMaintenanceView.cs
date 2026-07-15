namespace Whitebird.Domain.Features.Schedule;

public class ScheduleMaintenanceView
{
    public int ScheduleMaintenanceId { get; set; }
    public DateTime ScheduleDate { get; set; }
    public int AssetId { get; set; }
    public string? AssetCode { get; set; }
    public string? AssetName { get; set; }
    public int MaintenanceType { get; set; }
    public string? MaintenanceTypeName { get; set; }
    public int? MaintenanceAssetTransactionId { get; set; }
    public int? PostAssetTransactionId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}