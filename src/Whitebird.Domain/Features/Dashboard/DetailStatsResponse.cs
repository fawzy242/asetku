using Whitebird.Infra.Features.Common;

namespace Whitebird.Domain.Features.Dashboard;

/// <summary>
/// Response model for detail stats (drilldown)
/// </summary>
public class DetailStatsResponse
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public PaginatedResult<object> Data { get; set; } = new();
}