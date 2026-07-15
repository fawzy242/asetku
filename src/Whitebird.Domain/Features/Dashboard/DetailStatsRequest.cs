namespace Whitebird.Domain.Features.Dashboard;

/// <summary>
/// Request model for detail stats (drilldown)
/// </summary>
public class DetailStatsRequest
{
    public string Type { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}