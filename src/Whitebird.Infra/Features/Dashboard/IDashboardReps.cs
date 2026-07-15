using Whitebird.Domain.Features.Dashboard;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.Dashboard;

/// <summary>
/// Repository interface for Dashboard operations
/// </summary>
public interface IDashboardReps
{
    // Main stats
    Task<DashboardStatsViewModel> GetStatsAsync();

    // Charts
    Task<IEnumerable<MonthlyStatDto>> GetMonthlyStatsAsync(int year);
    Task<IEnumerable<CategoryBreakdownDto>> GetCategoryBreakdownAsync();

    // Recent transactions
    Task<IEnumerable<RecentTransactionDto>> GetRecentTransactionsAsync(int limit);

    // Expired warranty
    Task<IEnumerable<ExpiredWarrantyDto>> GetExpiredWarrantyAsync();

    // Schedule stats
    Task<ScheduleStatsDto> GetScheduleStatsAsync();

    // Detail Stats (drilldown) - Asset types
    Task<PaginatedResult<AssetDetailDto>> GetAvailableAssetsDetailAsync(
        int page, int pageSize, string? search, string? sortBy, bool sortDescending);

    Task<PaginatedResult<AssetDetailDto>> GetAssignedAssetsDetailAsync(
        int page, int pageSize, string? search, string? sortBy, bool sortDescending);

    Task<PaginatedResult<AssetDetailDto>> GetOnLoanAssetsDetailAsync(
        int page, int pageSize, string? search, string? sortBy, bool sortDescending);

    Task<PaginatedResult<AssetDetailDto>> GetInMaintenanceAssetsDetailAsync(
        int page, int pageSize, string? search, string? sortBy, bool sortDescending);

    Task<PaginatedResult<AssetDetailDto>> GetDisposedAssetsDetailAsync(
        int page, int pageSize, string? search, string? sortBy, bool sortDescending);

    Task<PaginatedResult<AssetDetailDto>> GetDamagedAssetsDetailAsync(
        int page, int pageSize, string? search, string? sortBy, bool sortDescending);

    Task<PaginatedResult<AssetDetailDto>> GetExpiredWarrantyDetailAsync(
        int page, int pageSize, string? search, string? sortBy, bool sortDescending);

    // Detail Stats (drilldown) - Overdue loans
    Task<PaginatedResult<OverdueLoanDetailDto>> GetOverdueLoansDetailAsync(
        int page, int pageSize, string? search, string? sortBy, bool sortDescending);
}