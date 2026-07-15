namespace Whitebird.Domain.Features.EmployeeSummary;

/// <summary>
/// Category stats for current holding chart
/// </summary>
public class EmployeeCategoryStatsDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Response for category stats
/// </summary>
public class EmployeeCategoryStatsResponse
{
    public int TotalAssets { get; set; }
    public List<EmployeeCategoryStatsDto> Data { get; set; } = new();
}