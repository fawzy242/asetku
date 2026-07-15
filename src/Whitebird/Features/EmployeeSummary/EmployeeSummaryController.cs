using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.EmployeeSummary;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.EmployeeSummary;
using Whitebird.Features.Common;

namespace Whitebird.Features.EmployeeSummary;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmployeeSummaryController : ControllerBase
{
    private readonly IEmployeeSummaryService _employeeSummaryService;

    public EmployeeSummaryController(IEmployeeSummaryService employeeSummaryService)
    {
        _employeeSummaryService = employeeSummaryService;
    }

    // ============================================================
    // LOOKUP / DROPDOWN
    // ============================================================

    /// <summary>
    /// Get employee data for lookup/dropdown
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    [HttpGet("lookup/{employeeId:int}")]
    public async Task<IActionResult> GetEmployeeData(int employeeId)
        => this.HandleResult(await _employeeSummaryService.GetEmployeeDataAsync(employeeId));

    // ============================================================
    // COMPLETE SUMMARY
    // ============================================================

    /// <summary>
    /// Get complete employee summary (header + stats + current holding + assets history + transaction history + photo + attachments)
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    [HttpGet("{employeeId:int}")]
    public async Task<IActionResult> GetSummary(int employeeId)
        => this.HandleResult(await _employeeSummaryService.GetSummaryAsync(employeeId));

    // ============================================================
    // STATISTICS
    // ============================================================

    /// <summary>
    /// Get employee statistics only (cards)
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    [HttpGet("{employeeId:int}/stats")]
    public async Task<IActionResult> GetStats(int employeeId)
        => this.HandleResult(await _employeeSummaryService.GetStatsAsync(employeeId));

    // ============================================================
    // CURRENT HOLDING
    // ============================================================

    /// <summary>
    /// Get current holding grid (assets currently held by employee)
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="search">Search keyword</param>
    /// <param name="sortBy">Sort column</param>
    /// <param name="sortDescending">Sort direction</param>
    [HttpGet("{employeeId:int}/current-holding")]
    public async Task<IActionResult> GetCurrentHolding(
        int employeeId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var request = new EmployeeSummaryRequest
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy,
            SortDescending = sortDescending
        };
        return this.HandleResult(await _employeeSummaryService.GetCurrentHoldingAsync(employeeId, request));
    }

    // ============================================================
    // ASSETS HISTORY
    // ============================================================

    /// <summary>
    /// Get assets history grid (assets previously held, now with another employee)
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="search">Search keyword</param>
    /// <param name="sortBy">Sort column</param>
    /// <param name="sortDescending">Sort direction</param>
    [HttpGet("{employeeId:int}/assets-history")]
    public async Task<IActionResult> GetAssetsHistory(
        int employeeId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var request = new EmployeeSummaryRequest
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy,
            SortDescending = sortDescending
        };
        return this.HandleResult(await _employeeSummaryService.GetAssetsHistoryAsync(employeeId, request));
    }

    // ============================================================
    // TRANSACTION HISTORY
    // ============================================================

    /// <summary>
    /// Get transaction history grid
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="search">Search keyword</param>
    /// <param name="sortBy">Sort column</param>
    /// <param name="sortDescending">Sort direction</param>
    [HttpGet("{employeeId:int}/transaction-history")]
    public async Task<IActionResult> GetTransactionHistory(
        int employeeId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var request = new EmployeeSummaryRequest
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy,
            SortDescending = sortDescending
        };
        return this.HandleResult(await _employeeSummaryService.GetTransactionHistoryAsync(employeeId, request));
    }

    // ============================================================
    // CURRENT HOLDING STATS (Chart)
    // ============================================================

    /// <summary>
    /// Get current holding category stats (chart)
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    [HttpGet("{employeeId:int}/current-holding-stats")]
    public async Task<IActionResult> GetCurrentHoldingStats(int employeeId)
        => this.HandleResult(await _employeeSummaryService.GetCurrentHoldingStatsAsync(employeeId));

    // ============================================================
    // TRANSACTION STATS (Chart)
    // ============================================================

    /// <summary>
    /// Get transaction stats (chart)
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    [HttpGet("{employeeId:int}/transaction-history-stats")]
    public async Task<IActionResult> GetTransactionStats(int employeeId)
        => this.HandleResult(await _employeeSummaryService.GetTransactionStatsAsync(employeeId));

    // ============================================================
    // SCHEDULE STATS
    // ============================================================

    /// <summary>
    /// Get schedule stats (handover and return) for employee
    /// </summary>
    /// <param name="employeeId">Employee ID</param>
    [HttpGet("{employeeId:int}/schedule-stats")]
    public async Task<IActionResult> GetScheduleStats(int employeeId)
        => this.HandleResult(await _employeeSummaryService.GetScheduleStatsAsync(employeeId));
}