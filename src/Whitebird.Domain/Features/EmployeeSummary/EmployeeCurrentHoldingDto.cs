namespace Whitebird.Domain.Features.EmployeeSummary;

/// <summary>
/// Current asset holding item (grid)
/// </summary>
public class EmployeeCurrentHoldingDto
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AssociationType { get; set; } = string.Empty;
    public DateTime SinceDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public bool IsOverdue { get; set; }
    public string? ConditionName { get; set; }
}