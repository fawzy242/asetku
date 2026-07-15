namespace Whitebird.Domain.Features.Dashboard;

/// <summary>
/// DTO for monthly statistics on dashboard
/// </summary>
public class MonthlyStatDto
{
    public int Month { get; set; }
    public int TransactionCount { get; set; }
    public int UniqueAssetsCount { get; set; }
}