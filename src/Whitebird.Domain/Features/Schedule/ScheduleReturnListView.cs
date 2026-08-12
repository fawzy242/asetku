namespace Whitebird.Domain.Features.Schedule;

public class ScheduleReturnListView
{
    public int ScheduleReturnId { get; set; }
    public DateTime ScheduleDate { get; set; }
    public int AssetId { get; set; }
    public string? AssetCode { get; set; }
    public string? AssetName { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int? ReturnAssetTransactionId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";

    // Action flags untuk grid
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}