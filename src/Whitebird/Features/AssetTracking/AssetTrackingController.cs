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
    // SCHEDULE STATS - Using GridRequest
    // ============================================================

    [HttpGet("{assetId:int}/schedule-stats")]
    public async Task<IActionResult> GetScheduleStats([FromQuery] AssetTrackingGridRequest request)
        => this.HandleResult(await _assetTrackingService.GetScheduleStatsAsync(request));

    // ============================================================
    // TRANSACTION TYPE STATS
    // ============================================================

    [HttpGet("{assetId:int}/transactiontype-stats")]
    public async Task<IActionResult> GetTransactionTypeStats(int assetId)
        => this.HandleResult(await _assetTrackingService.GetTransactionTypeStatsAsync(assetId));

    // ============================================================
    // EMPLOYEE HISTORY - Using GridRequest
    // ============================================================

    [HttpGet("{assetId:int}/employee-history")]
    public async Task<IActionResult> GetEmployeeHistory([FromQuery] AssetTrackingGridRequest request)
        => this.HandleResult(await _assetTrackingService.GetEmployeeHistoryAsync(request));

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