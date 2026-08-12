using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Dashboard;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Dashboard;

/// <summary>
/// Service interface for Dashboard business logic
/// </summary>
public interface IDashboardService
{
    // ============================================================
    // MAIN STATS
    // ============================================================

    Task<ServiceResult<DashboardStatsViewModel>> GetStatsAsync();

    // ============================================================
    // CHARTS
    // ============================================================

    Task<ServiceResult<IEnumerable<MonthlyStatDto>>> GetMonthlyStatsAsync(int year);
    Task<ServiceResult<IEnumerable<CategoryBreakdownDto>>> GetCategoryBreakdownAsync();

    // ============================================================
    // RECENT TRANSACTIONS
    // ============================================================

    Task<ServiceResult<IEnumerable<RecentTransactionDto>>> GetRecentTransactionsAsync(int limit);

    // ============================================================
    // EXPIRED WARRANTY
    // ============================================================

    Task<ServiceResult<IEnumerable<ExpiredWarrantyDto>>> GetExpiredWarrantyAsync();

    // ============================================================
    // SCHEDULE STATS
    // ============================================================

    Task<ServiceResult<ScheduleStatsDto>> GetScheduleStatsAsync();

    // ============================================================
    // DETAIL STATS (Drilldown) - Using GridRequest
    // ============================================================

    Task<ServiceResult<DetailStatsResponse>> GetDetailStatsAsync(DashboardDetailStatsRequest request);
}