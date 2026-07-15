namespace Whitebird.Domain.Features.AssetTracking;

/// <summary>
/// Transaction type composition for a specific asset (chart data)
/// </summary>
public class AssetTrackingTransactionTypeStatsDto
{
    public string TransactionType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Response for transaction type stats
/// </summary>
public class AssetTrackingTransactionTypeStatsResponse
{
    public int TotalTransactions { get; set; }
    public List<AssetTrackingTransactionTypeStatsDto> Data { get; set; } = new();
}