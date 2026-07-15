namespace Whitebird.Domain.Features.AssetTracking;

/// <summary>
/// Employee history for a specific asset (grid data)
/// </summary>
public class AssetTrackingEmployeeHistoryDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public int TotalTransactions { get; set; }
    public int AssetsHeld { get; set; }
}