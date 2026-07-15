namespace Whitebird.Domain.Features.Dashboard;

/// <summary>
/// DTO for category breakdown on dashboard
/// </summary>
public class CategoryBreakdownDto
{
    public string Category { get; set; } = string.Empty;
    public int AssetCount { get; set; }
    public decimal TotalValue { get; set; }
}