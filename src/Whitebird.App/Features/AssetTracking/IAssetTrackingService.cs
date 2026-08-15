using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Asset;
using Whitebird.Domain.Features.AssetTracking;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.AssetTracking;

/// <summary>
/// Service interface for Asset Tracking business logic
/// </summary>
public interface IAssetTrackingService
{
    // ============================================================
    // LOOKUP
    // ============================================================

    Task<ServiceResult<AssetTrackingGetDataDto>> GetAssetDataAsync(int assetId);

    // ============================================================
    // DETAIL TRACKING
    // ============================================================

    Task<ServiceResult<AssetTrackingView>> GetTrackingDetailAsync(int assetId);

    // ============================================================
    // STATS HEADER
    // ============================================================

    Task<ServiceResult<AssetStatsView>> GetAssetStatsAsync(int assetId);

    // ============================================================
    // TIMELINE
    // ============================================================

    Task<ServiceResult<IEnumerable<AssetTimelineEntry>>> GetTimelineAsync(int assetId);

    // ============================================================
    // SCHEDULE STATS - Using GridRequest
    // ============================================================

    Task<ServiceResult<PaginatedResult<AssetTrackingScheduleStatsDto>>> GetScheduleStatsAsync(AssetTrackingGridRequest request);

    // ============================================================
    // TRANSACTION TYPE STATS
    // ============================================================

    Task<ServiceResult<AssetTrackingTransactionTypeStatsResponse>> GetTransactionTypeStatsAsync(int assetId);

    // ============================================================
    // EMPLOYEE HISTORY - Using GridRequest
    // ============================================================

    Task<ServiceResult<PaginatedResult<AssetTrackingEmployeeHistoryDto>>> GetEmployeeHistoryAsync(AssetTrackingGridRequest request);

    // ============================================================
    // NEW: PHOTO & ATTACHMENTS (delegated to AssetService)
    // ============================================================

    /// <summary>
    /// Gets profile photo bytes for an asset (delegated to AssetService)
    /// </summary>
    Task<ServiceResult<byte[]>> GetPhotoAsync(int assetId);

    /// <summary>
    /// Gets attachment list for an asset (delegated to AssetService)
    /// </summary>
    Task<ServiceResult<IEnumerable<AssetAttachmentViewDto>>> GetAttachmentsAsync(int assetId);
}