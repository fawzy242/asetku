using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Dashboard;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Dashboard;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Dashboard;

/// <summary>
/// Service implementation for Dashboard business logic
/// </summary>
public class DashboardService : BaseService, IDashboardService
{
    private readonly IDashboardReps _dashboardReps;

    public DashboardService(
        IDashboardReps dashboardReps,
        ILogger<DashboardService> logger) : base(logger)
    {
        _dashboardReps = dashboardReps;
    }

    // ============================================================
    // MAIN STATS
    // ============================================================

    public async Task<ServiceResult<DashboardStatsViewModel>> GetStatsAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var stats = await _dashboardReps.GetStatsAsync();
            return ServiceResult<DashboardStatsViewModel>.Success(stats);
        }, "get dashboard stats");
    }

    // ============================================================
    // CHARTS
    // ============================================================

    public async Task<ServiceResult<IEnumerable<MonthlyStatDto>>> GetMonthlyStatsAsync(int year)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            if (year <= 0) year = DateTime.Now.Year;
            var data = await _dashboardReps.GetMonthlyStatsAsync(year);
            return ServiceResult<IEnumerable<MonthlyStatDto>>.Success(data);
        }, "get monthly stats");
    }

    public async Task<ServiceResult<IEnumerable<CategoryBreakdownDto>>> GetCategoryBreakdownAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _dashboardReps.GetCategoryBreakdownAsync();
            return ServiceResult<IEnumerable<CategoryBreakdownDto>>.Success(data);
        }, "get category breakdown");
    }

    // ============================================================
    // RECENT TRANSACTIONS
    // ============================================================

    public async Task<ServiceResult<IEnumerable<RecentTransactionDto>>> GetRecentTransactionsAsync(int limit)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            if (limit <= 0) limit = 10;
            var data = await _dashboardReps.GetRecentTransactionsAsync(limit);
            return ServiceResult<IEnumerable<RecentTransactionDto>>.Success(data);
        }, "get recent transactions");
    }

    // ============================================================
    // EXPIRED WARRANTY
    // ============================================================

    public async Task<ServiceResult<IEnumerable<ExpiredWarrantyDto>>> GetExpiredWarrantyAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _dashboardReps.GetExpiredWarrantyAsync();
            return ServiceResult<IEnumerable<ExpiredWarrantyDto>>.Success(data);
        }, "get expired warranty");
    }

    // ============================================================
    // SCHEDULE STATS
    // ============================================================

    public async Task<ServiceResult<ScheduleStatsDto>> GetScheduleStatsAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var data = await _dashboardReps.GetScheduleStatsAsync();
            return ServiceResult<ScheduleStatsDto>.Success(data);
        }, "get schedule stats");
    }

    // ============================================================
    // DETAIL STATS (Drilldown) - Using GridRequest
    // ============================================================

    public async Task<ServiceResult<DetailStatsResponse>> GetDetailStatsAsync(DashboardDetailStatsRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(request.Type))
            {
                return ServiceResult<DetailStatsResponse>.BadRequest("Type is required");
            }

            var validTypes = new[] { "available", "assigned", "onloan", "inmaintenance", "disposed", "damaged", "expiredwarranty", "overdueloans" };
            if (!validTypes.Contains(request.Type.ToLower()))
            {
                return ServiceResult<DetailStatsResponse>.BadRequest(
                    $"Invalid type: {request.Type}. Valid types: Available, Assigned, OnLoan, InMaintenance, Disposed, Damaged, ExpiredWarranty, OverdueLoans");
            }

            var result = await _dashboardReps.GetDetailStatsAsync(request);

            var title = request.Type switch
            {
                "Available" or "available" => "Available Assets",
                "Assigned" or "assigned" => "Assigned Assets",
                "OnLoan" or "onloan" => "Assets On Loan",
                "InMaintenance" or "inmaintenance" => "Assets In Maintenance",
                "Disposed" or "disposed" => "Disposed Assets",
                "Damaged" or "damaged" => "Damaged Assets",
                "ExpiredWarranty" or "expiredwarranty" => "Expired Warranty Assets",
                "OverdueLoans" or "overdueloans" => "Overdue Loans",
                _ => request.Type
            };

            var response = new DetailStatsResponse
            {
                Type = request.Type,
                Title = title,
                TotalCount = result.TotalCount,
                Data = new PaginatedResult<object>
                {
                    Data = result.Data.Cast<object>().ToList(),
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalPages = result.TotalPages
                }
            };

            return ServiceResult<DetailStatsResponse>.Success(response);
        }, "get detail stats");
    }
}