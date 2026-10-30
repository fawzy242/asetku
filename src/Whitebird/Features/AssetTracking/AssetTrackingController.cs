using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whitebird.App.Features.AssetTracking;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.AssetTracking;
using Whitebird.Features.Common;

namespace Whitebird.Features.AssetTracking;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class AssetTrackingController : ControllerBase
{
    private readonly IAssetTrackingService _assetTrackingService;

    public AssetTrackingController(IAssetTrackingService assetTrackingService)
    {
        _assetTrackingService = assetTrackingService;
    }

    // ============================================================
    // LOOKUP
    // ============================================================

    [HttpGet("lookup/{assetId:int}")]
    public async Task<IActionResult> GetAssetData(int assetId)
        => this.HandleResult(await _assetTrackingService.GetAssetDataAsync(assetId));

    // ============================================================
    // DETAIL TRACKING
    // ============================================================

    [HttpGet("{assetId:int}")]
    public async Task<IActionResult> GetTrackingDetail(int assetId)
        => this.HandleResult(await _assetTrackingService.GetTrackingDetailAsync(assetId));

    // ============================================================
    // STATS HEADER
    // ============================================================

    [HttpGet("{assetId:int}/stats")]
    public async Task<IActionResult> GetAssetStats(int assetId)
        => this.HandleResult(await _assetTrackingService.GetAssetStatsAsync(assetId));

    // ============================================================
    // TIMELINE
    // ============================================================

    [HttpGet("{assetId:int}/timeline")]
    public async Task<IActionResult> GetTimeline(int assetId)
        => this.HandleResult(await _assetTrackingService.GetTimelineAsync(assetId));

    // ============================================================
    // SCHEDULE STATS — route assetId is the source of truth
    // ============================================================
    //
    // Pre-H7 the {assetId:int} route parameter was declared but ignored; the
    // service read request.AssetId from the querystring. A client that put the
    // ID only in the route received 0 rows silently. The route is now bound
    // into the request before delegation, matching GetTrackingDetail /
    // GetTimeline / GetTransactionTypeStats / GetPhoto / GetAttachments.
    //
    // P3-2: The schedule type is supplied via the "scheduleType" query parameter
    // (supported values: "handover", "return", "maintenance"). For backward
    // compatibility, the legacy "search" query parameter continues to work as
    // a fallback selector; new clients should use "scheduleType".

    [HttpGet("{assetId:int}/schedule-stats")]
    public async Task<IActionResult> GetScheduleStats(int assetId, [FromQuery] AssetTrackingGridRequest request)
    {
        request.AssetId = assetId;
        return this.HandleResult(await _assetTrackingService.GetScheduleStatsAsync(request));
    }

    // ============================================================
    // TRANSACTION TYPE STATS
    // ============================================================

    [HttpGet("{assetId:int}/transactiontype-stats")]
    public async Task<IActionResult> GetTransactionTypeStats(int assetId)
        => this.HandleResult(await _assetTrackingService.GetTransactionTypeStatsAsync(assetId));

    // ============================================================
    // EMPLOYEE HISTORY — route assetId is the source of truth
    // ============================================================
    //
    // Same fix as schedule-stats above: the route parameter is bound into the
    // request so a route-only client no longer receives 0 rows.

    [HttpGet("{assetId:int}/employee-history")]
    public async Task<IActionResult> GetEmployeeHistory(int assetId, [FromQuery] AssetTrackingGridRequest request)
    {
        request.AssetId = assetId;
        return this.HandleResult(await _assetTrackingService.GetEmployeeHistoryAsync(request));
    }

    // ============================================================
    // PHOTO & ATTACHMENTS (delegated to AssetService)
    // ============================================================

    [HttpGet("{assetId:int}/photo")]
    public async Task<IActionResult> GetPhoto(int assetId)
    {
        var result = await _assetTrackingService.GetPhotoAsync(assetId);
        if (!result.IsSuccess || result.Data == null)
            return this.HandleResult(result);

        return File(result.Data.Bytes, result.Data.ContentType);
    }

    [HttpGet("{assetId:int}/attachments")]
    public async Task<IActionResult> GetAttachments(int assetId)
        => this.HandleResult(await _assetTrackingService.GetAttachmentsAsync(assetId));
}