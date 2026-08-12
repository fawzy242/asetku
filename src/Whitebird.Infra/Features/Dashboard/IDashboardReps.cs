using Whitebird.Domain.Features.Dashboard;
using Whitebird.Infra.Features.Common;
using Whitebird.Domain.Features.Common;

namespace Whitebird.Infra.Features.Dashboard;

/// <summary>
/// Repository interface for Dashboard operations
/// </summary>
public interface IDashboardReps
{
    // ============================================================
    // MAIN STATS
    // ============================================================

    Task<DashboardStatsViewModel> GetStatsAsync();

    // ============================================================
    // CHARTS
    // ============================================================

    Task<IEnumerable<MonthlyStatDto>> GetMonthlyStatsAsync(int year);
    Task<IEnumerable<CategoryBreakdownDto>> GetCategoryBreakdownAsync();

    // ============================================================
    // RECENT TRANSACTIONS
    // ============================================================

    Task<IEnumerable<RecentTransactionDto>> GetRecentTransactionsAsync(int limit);

    // ============================================================
    // EXPIRED WARRANTY
    // ============================================================

    Task<IEnumerable<ExpiredWarrantyDto>> GetExpiredWarrantyAsync();

    // ============================================================
    // SCHEDULE STATS
    // ============================================================

    Task<ScheduleStatsDto> GetScheduleStatsAsync();

    // ============================================================
    // DETAIL STATS (Drilldown) - Using GridRequest
    // ============================================================

    Task<PaginatedResult<AssetDetailDto>> GetDetailStatsAsync(DashboardDetailStatsRequest request);
}