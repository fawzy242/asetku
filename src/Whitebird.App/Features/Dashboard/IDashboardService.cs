using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Dashboard;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Dashboard;

/// <summary>
/// Service interface for Dashboard business logic
/// </summary>
public interface IDashboardService
{
    // Main stats
    Task<ServiceResult<DashboardStatsViewModel>> GetStatsAsync();

    // Charts
    Task<ServiceResult<IEnumerable<MonthlyStatDto>>> GetMonthlyStatsAsync(int year);
    Task<ServiceResult<IEnumerable<CategoryBreakdownDto>>> GetCategoryBreakdownAsync();

    // Recent transactions
    Task<ServiceResult<IEnumerable<RecentTransactionDto>>> GetRecentTransactionsAsync(int limit);

    // Expired warranty
    Task<ServiceResult<IEnumerable<ExpiredWarrantyDto>>> GetExpiredWarrantyAsync();

    // Schedule stats
    Task<ServiceResult<ScheduleStatsDto>> GetScheduleStatsAsync();

    // Detail Stats (drilldown)
    Task<ServiceResult<DetailStatsResponse>> GetDetailStatsAsync(DetailStatsRequest request);
}