namespace Whitebird.Domain.Features.Reports;

/// <summary>
/// DTO for monthly statistics on dashboard
/// </summary>
public class MonthlyStatDto
{
    public int Month { get; set; }
    public int TransactionCount { get; set; }
    public int UniqueAssetsCount { get; set; }
}

/// <summary>
/// DTO for category breakdown on dashboard
/// </summary>
public class CategoryBreakdownDto
{
    public string Category { get; set; } = string.Empty;
    public int AssetCount { get; set; }
    public decimal TotalValue { get; set; }
}