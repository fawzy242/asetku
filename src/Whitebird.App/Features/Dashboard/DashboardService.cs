using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Dashboard;
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
    // DETAIL STATS (DRILLDOWN)
    // ============================================================

    public async Task<ServiceResult<DetailStatsResponse>> GetDetailStatsAsync(DetailStatsRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(request.Type))
            {
                return ServiceResult<DetailStatsResponse>.BadRequest("Type is required");
            }

            PaginatedResult<object>? result = null;
            string title = string.Empty;
            int totalCount = 0;

            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            switch (request.Type.ToLower())
            {
                case "available":
                    var available = await _dashboardReps.GetAvailableAssetsDetailAsync(page, pageSize, request.Search, request.SortBy, request.SortDescending);
                    result = new PaginatedResult<object>
                    {
                        Data = available.Data.Cast<object>().ToList(),
                        TotalCount = available.TotalCount,
                        PageNumber = available.PageNumber,
                        PageSize = available.PageSize,
                        TotalPages = available.TotalPages
                    };
                    title = "Available Assets";
                    totalCount = available.TotalCount;
                    break;

                case "assigned":
                    var assigned = await _dashboardReps.GetAssignedAssetsDetailAsync(page, pageSize, request.Search, request.SortBy, request.SortDescending);
                    result = new PaginatedResult<object>
                    {
                        Data = assigned.Data.Cast<object>().ToList(),
                        TotalCount = assigned.TotalCount,
                        PageNumber = assigned.PageNumber,
                        PageSize = assigned.PageSize,
                        TotalPages = assigned.TotalPages
                    };
                    title = "Assigned Assets";
                    totalCount = assigned.TotalCount;
                    break;

                case "onloan":
                    var onLoan = await _dashboardReps.GetOnLoanAssetsDetailAsync(page, pageSize, request.Search, request.SortBy, request.SortDescending);
                    result = new PaginatedResult<object>
                    {
                        Data = onLoan.Data.Cast<object>().ToList(),
                        TotalCount = onLoan.TotalCount,
                        PageNumber = onLoan.PageNumber,
                        PageSize = onLoan.PageSize,
                        TotalPages = onLoan.TotalPages
                    };
                    title = "Assets On Loan";
                    totalCount = onLoan.TotalCount;
                    break;

                case "inmaintenance":
                    var inMaintenance = await _dashboardReps.GetInMaintenanceAssetsDetailAsync(page, pageSize, request.Search, request.SortBy, request.SortDescending);
                    result = new PaginatedResult<object>
                    {
                        Data = inMaintenance.Data.Cast<object>().ToList(),
                        TotalCount = inMaintenance.TotalCount,
                        PageNumber = inMaintenance.PageNumber,
                        PageSize = inMaintenance.PageSize,
                        TotalPages = inMaintenance.TotalPages
                    };
                    title = "Assets In Maintenance";
                    totalCount = inMaintenance.TotalCount;
                    break;

                case "disposed":
                    var disposed = await _dashboardReps.GetDisposedAssetsDetailAsync(page, pageSize, request.Search, request.SortBy, request.SortDescending);
                    result = new PaginatedResult<object>
                    {
                        Data = disposed.Data.Cast<object>().ToList(),
                        TotalCount = disposed.TotalCount,
                        PageNumber = disposed.PageNumber,
                        PageSize = disposed.PageSize,
                        TotalPages = disposed.TotalPages
                    };
                    title = "Disposed Assets";
                    totalCount = disposed.TotalCount;
                    break;

                case "damaged":
                    var damaged = await _dashboardReps.GetDamagedAssetsDetailAsync(page, pageSize, request.Search, request.SortBy, request.SortDescending);
                    result = new PaginatedResult<object>
                    {
                        Data = damaged.Data.Cast<object>().ToList(),
                        TotalCount = damaged.TotalCount,
                        PageNumber = damaged.PageNumber,
                        PageSize = damaged.PageSize,
                        TotalPages = damaged.TotalPages
                    };
                    title = "Damaged Assets";
                    totalCount = damaged.TotalCount;
                    break;

                case "expiredwarranty":
                    var expiredWarranty = await _dashboardReps.GetExpiredWarrantyDetailAsync(page, pageSize, request.Search, request.SortBy, request.SortDescending);
                    result = new PaginatedResult<object>
                    {
                        Data = expiredWarranty.Data.Cast<object>().ToList(),
                        TotalCount = expiredWarranty.TotalCount,
                        PageNumber = expiredWarranty.PageNumber,
                        PageSize = expiredWarranty.PageSize,
                        TotalPages = expiredWarranty.TotalPages
                    };
                    title = "Expired Warranty Assets";
                    totalCount = expiredWarranty.TotalCount;
                    break;

                case "overdueloans":
                    var overdueLoans = await _dashboardReps.GetOverdueLoansDetailAsync(page, pageSize, request.Search, request.SortBy, request.SortDescending);
                    result = new PaginatedResult<object>
                    {
                        Data = overdueLoans.Data.Cast<object>().ToList(),
                        TotalCount = overdueLoans.TotalCount,
                        PageNumber = overdueLoans.PageNumber,
                        PageSize = overdueLoans.PageSize,
                        TotalPages = overdueLoans.TotalPages
                    };
                    title = "Overdue Loans";
                    totalCount = overdueLoans.TotalCount;
                    break;

                default:
                    return ServiceResult<DetailStatsResponse>.BadRequest(
                        $"Invalid type: {request.Type}. Valid types: Available, Assigned, OnLoan, InMaintenance, Disposed, Damaged, ExpiredWarranty, OverdueLoans");
            }

            var response = new DetailStatsResponse
            {
                Type = request.Type,
                Title = title,
                TotalCount = totalCount,
                Data = result!
            };

            return ServiceResult<DetailStatsResponse>.Success(response);
        }, "get detail stats");
    }
}