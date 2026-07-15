namespace Whitebird.Domain.Features.EmployeeSummary;

/// <summary>
/// Employee statistics (cards)
/// </summary>
public class EmployeeSummaryStatsDto
{
    public int CurrentlyHeldAssets { get; set; }
    public int AssetsOnLoan { get; set; }
    public int OverdueLoans { get; set; }
    public int TotalHistoricalAssets { get; set; }
    public int ReturnedAssets { get; set; }
    public int DamagedReturns { get; set; }
}