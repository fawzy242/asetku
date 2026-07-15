namespace Whitebird.Domain.Features.EmployeeSummary;

/// <summary>
/// Assets that were previously held but now held by another employee (grid)
/// </summary>
public class EmployeeAssetsHistoryDto
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime LastHeldDate { get; set; }
    public string? CurrentHolderName { get; set; }
    public int? CurrentHolderId { get; set; }
    public string? ConditionName { get; set; }
}