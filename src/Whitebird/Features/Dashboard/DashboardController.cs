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

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
        => this.HandleResult(await _dashboardService.GetStatsAsync());

    // ============================================================
    // CHARTS
    // ============================================================

    [HttpGet("monthly-stats")]
    public async Task<IActionResult> GetMonthlyStats([FromQuery] int year = 0)
        => this.HandleResult(await _dashboardService.GetMonthlyStatsAsync(year));

    [HttpGet("category-breakdown")]
    public async Task<IActionResult> GetCategoryBreakdown()
        => this.HandleResult(await _dashboardService.GetCategoryBreakdownAsync());

    // ============================================================
    // RECENT TRANSACTIONS
    // ============================================================

    [HttpGet("recent-transactions")]
    public async Task<IActionResult> GetRecentTransactions([FromQuery] int limit = 10)
        => this.HandleResult(await _dashboardService.GetRecentTransactionsAsync(limit));

    // ============================================================
    // EXPIRED WARRANTY
    // ============================================================

    [HttpGet("expired-warranty")]
    public async Task<IActionResult> GetExpiredWarranty()
        => this.HandleResult(await _dashboardService.GetExpiredWarrantyAsync());

    // ============================================================
    // SCHEDULE STATS
    // ============================================================

    [HttpGet("schedule-stats")]
    public async Task<IActionResult> GetScheduleStats()
        => this.HandleResult(await _dashboardService.GetScheduleStatsAsync());

    // ============================================================
    // DETAIL STATS (Drilldown) - Using GridRequest
    // ============================================================

    [HttpGet("detail-stats")]
    public async Task<IActionResult> GetDetailStats([FromQuery] DashboardDetailStatsRequest request)
        => this.HandleResult(await _dashboardService.GetDetailStatsAsync(request));
}