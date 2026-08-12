using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.EmployeeSummary;
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
    // LOOKUP
    // ============================================================

    [HttpGet("lookup/{employeeId:int}")]
    public async Task<IActionResult> GetEmployeeData(int employeeId)
        => this.HandleResult(await _employeeSummaryService.GetEmployeeDataAsync(employeeId));

    // ============================================================
    // COMPLETE SUMMARY
    // ============================================================

    [HttpGet("{employeeId:int}")]
    public async Task<IActionResult> GetSummary(int employeeId)
        => this.HandleResult(await _employeeSummaryService.GetSummaryAsync(employeeId));

    // ============================================================
    // STATISTICS
    // ============================================================

    [HttpGet("{employeeId:int}/stats")]
    public async Task<IActionResult> GetStats(int employeeId)
        => this.HandleResult(await _employeeSummaryService.GetStatsAsync(employeeId));

    // ============================================================
    // CURRENT HOLDING - Using GridRequest
    // ============================================================

    [HttpGet("{employeeId:int}/current-holding")]
    public async Task<IActionResult> GetCurrentHolding([FromQuery] EmployeeSummaryGridRequest request)
        => this.HandleResult(await _employeeSummaryService.GetCurrentHoldingAsync(request));

    // ============================================================
    // ASSETS HISTORY - Using GridRequest
    // ============================================================

    [HttpGet("{employeeId:int}/assets-history")]
    public async Task<IActionResult> GetAssetsHistory([FromQuery] EmployeeSummaryGridRequest request)
        => this.HandleResult(await _employeeSummaryService.GetAssetsHistoryAsync(request));

    // ============================================================
    // TRANSACTION HISTORY - Using GridRequest
    // ============================================================

    [HttpGet("{employeeId:int}/transaction-history")]
    public async Task<IActionResult> GetTransactionHistory([FromQuery] EmployeeSummaryGridRequest request)
        => this.HandleResult(await _employeeSummaryService.GetTransactionHistoryAsync(request));

    // ============================================================
    // CURRENT HOLDING STATS (Chart)
    // ============================================================

    [HttpGet("{employeeId:int}/current-holding-stats")]
    public async Task<IActionResult> GetCurrentHoldingStats(int employeeId)
        => this.HandleResult(await _employeeSummaryService.GetCurrentHoldingStatsAsync(employeeId));

    // ============================================================
    // TRANSACTION STATS (Chart)
    // ============================================================

    [HttpGet("{employeeId:int}/transaction-history-stats")]
    public async Task<IActionResult> GetTransactionStats(int employeeId)
        => this.HandleResult(await _employeeSummaryService.GetTransactionStatsAsync(employeeId));

    // ============================================================
    // SCHEDULE STATS
    // ============================================================

    [HttpGet("{employeeId:int}/schedule-stats")]
    public async Task<IActionResult> GetScheduleStats(int employeeId)
        => this.HandleResult(await _employeeSummaryService.GetScheduleStatsAsync(employeeId));
}