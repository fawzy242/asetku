using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.AssetTracking;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.AssetTracking;
using Whitebird.Features.Common;

namespace Whitebird.Features.AssetTracking;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AssetTrackingController : ControllerBase
{
    private readonly IAssetTrackingService _assetTrackingService;

    public AssetTrackingController(IAssetTrackingService assetTrackingService)
    {
        _assetTrackingService = assetTrackingService;
    }

    // ============================================================
    // LOOKUP / DROPDOWN
    // ============================================================

    /// <summary>
    /// Get asset data for lookup/dropdown
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    [HttpGet("lookup/{assetId:int}")]
    public async Task<IActionResult> GetAssetData(int assetId)
        => this.HandleResult(await _assetTrackingService.GetAssetDataAsync(assetId));

    // ============================================================
    // DETAIL TRACKING
    // ============================================================

    /// <summary>
    /// Get complete tracking detail (header + timeline + software + photo + attachments)
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    [HttpGet("{assetId:int}")]
    public async Task<IActionResult> GetTrackingDetail(int assetId)
        => this.HandleResult(await _assetTrackingService.GetTrackingDetailAsync(assetId));

    // ============================================================
    // STATS HEADER
    // ============================================================

    /// <summary>
    /// Get asset header stats (without timeline)
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    [HttpGet("{assetId:int}/stats")]
    public async Task<IActionResult> GetAssetStats(int assetId)
        => this.HandleResult(await _assetTrackingService.GetAssetStatsAsync(assetId));

    // ============================================================
    // TIMELINE
    // ============================================================

    /// <summary>
    /// Get asset timeline/transaction history (checkpoint)
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    [HttpGet("{assetId:int}/timeline")]
    public async Task<IActionResult> GetTimeline(int assetId)
        => this.HandleResult(await _assetTrackingService.GetTimelineAsync(assetId));

    // ============================================================
    // SCHEDULE STATS
    // ============================================================

    /// <summary>
    /// Get schedule stats for a specific asset (grid)
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    /// <param name="type">Schedule type: handover, return, maintenance</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="search">Search keyword</param>
    [HttpGet("{assetId:int}/schedule-stats")]
    public async Task<IActionResult> GetScheduleStats(
        int assetId,
        [FromQuery] string type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        return this.HandleResult(await _assetTrackingService.GetScheduleStatsAsync(assetId, type, page, pageSize, search));
    }

    // ============================================================
    // TRANSACTION TYPE STATS
    // ============================================================

    /// <summary>
    /// Get transaction type composition for a specific asset (chart)
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    [HttpGet("{assetId:int}/transactiontype-stats")]
    public async Task<IActionResult> GetTransactionTypeStats(int assetId)
        => this.HandleResult(await _assetTrackingService.GetTransactionTypeStatsAsync(assetId));

    // ============================================================
    // EMPLOYEE HISTORY
    // ============================================================

    /// <summary>
    /// Get employee history for a specific asset (grid)
    /// </summary>
    /// <param name="assetId">Asset ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="search">Search keyword</param>
    [HttpGet("{assetId:int}/employee-history")]
    public async Task<IActionResult> GetEmployeeHistory(
        int assetId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        return this.HandleResult(await _assetTrackingService.GetEmployeeHistoryAsync(assetId, page, pageSize, search));
    }
}