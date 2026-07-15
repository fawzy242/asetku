using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.Dashboard;
using Whitebird.Domain.Features.Dashboard;
using Whitebird.Features.Common;

namespace Whitebird.Features.Dashboard;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // ============================================================
    // MAIN STATS
    // ============================================================

    /// <summary>
    /// Get all dashboard statistics (cards, alerts, counts)
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
        => this.HandleResult(await _dashboardService.GetStatsAsync());

    // ============================================================
    // CHARTS
    // ============================================================

    /// <summary>
    /// Get monthly statistics for chart
    /// </summary>
    /// <param name="year">Year (default: current year)</param>
    [HttpGet("monthly-stats")]
    public async Task<IActionResult> GetMonthlyStats([FromQuery] int year = 0)
        => this.HandleResult(await _dashboardService.GetMonthlyStatsAsync(year));

    /// <summary>
    /// Get category breakdown for chart
    /// </summary>
    [HttpGet("category-breakdown")]
    public async Task<IActionResult> GetCategoryBreakdown()
        => this.HandleResult(await _dashboardService.GetCategoryBreakdownAsync());

    // ============================================================
    // RECENT TRANSACTIONS
    // ============================================================

    /// <summary>
    /// Get recent transactions
    /// </summary>
    /// <param name="limit">Number of transactions (default: 10)</param>
    [HttpGet("recent-transactions")]
    public async Task<IActionResult> GetRecentTransactions([FromQuery] int limit = 10)
        => this.HandleResult(await _dashboardService.GetRecentTransactionsAsync(limit));

    // ============================================================
    // EXPIRED WARRANTY
    // ============================================================

    /// <summary>
    /// Get list of assets with expired warranty
    /// </summary>
    [HttpGet("expired-warranty")]
    public async Task<IActionResult> GetExpiredWarranty()
        => this.HandleResult(await _dashboardService.GetExpiredWarrantyAsync());

    // ============================================================
    // SCHEDULE STATS
    // ============================================================

    /// <summary>
    /// Get schedule statistics (Handover, Return, Maintenance)
    /// </summary>
    [HttpGet("schedule-stats")]
    public async Task<IActionResult> GetScheduleStats()
        => this.HandleResult(await _dashboardService.GetScheduleStatsAsync());

    // ============================================================
    // DETAIL STATS (DRILLDOWN)
    // ============================================================

    /// <summary>
    /// Get detail data for drilldown card
    /// </summary>
    /// <param name="type">Available, Assigned, OnLoan, InMaintenance, Disposed, Damaged, ExpiredWarranty, OverdueLoans</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="search">Search keyword</param>
    /// <param name="sortBy">Sort column</param>
    /// <param name="sortDescending">Sort direction</param>
    [HttpGet("detail-stats")]
    public async Task<IActionResult> GetDetailStats(
        [FromQuery] string type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var request = new DetailStatsRequest
        {
            Type = type,
            Page = page,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        return this.HandleResult(await _dashboardService.GetDetailStatsAsync(request));
    }
}