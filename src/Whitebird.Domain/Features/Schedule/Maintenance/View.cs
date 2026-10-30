namespace Whitebird.Domain.Features.Schedule;

public class ScheduleMaintenanceListView
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

    // Action flags untuk grid
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

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