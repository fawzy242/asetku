namespace Whitebird.Domain.Features.EmployeeSummary;

/// <summary>
/// Transaction stats for transaction history chart
/// </summary>
public class EmployeeTransactionStatsDto
{
    public string TransactionType { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Response for transaction stats
/// </summary>
public class EmployeeTransactionStatsResponse
{
    public int TotalTransactions { get; set; }
    public List<EmployeeTransactionStatsDto> Data { get; set; } = new();
}