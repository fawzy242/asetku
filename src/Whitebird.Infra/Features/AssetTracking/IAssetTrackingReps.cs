using Whitebird.Domain.Features.AssetTracking;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.AssetTracking;

/// <summary>
/// Repository interface for Asset Tracking operations
/// </summary>
public interface IAssetTrackingReps
{
    // ============================================================
    // LOOKUP / DROPDOWN
    // ============================================================

    Task<AssetTrackingGetDataDto?> GetAssetDataAsync(int assetId);

    // ============================================================
    // DETAIL TRACKING
    // ============================================================

    Task<AssetTrackingView?> GetTrackingDetailAsync(int assetId);

    // ============================================================
    // STATS HEADER
    // ============================================================

    Task<AssetStatsView?> GetAssetStatsAsync(int assetId);

    // ============================================================
    // TIMELINE
    // ============================================================

    Task<IEnumerable<AssetTimelineEntry>> GetTimelineAsync(int assetId);

    // ============================================================
    // SOFTWARE INSTALLATION
    // ============================================================

    Task<AssetTrackingSoftwareInstallationDto?> GetSoftwareInstallationByAssetIdAsync(int assetId);

    // ============================================================
    // ATTACHMENTS
    // ============================================================

    Task<IEnumerable<AssetTrackingAttachmentDto>> GetAttachmentsByAssetIdAsync(int assetId);

    // ============================================================
    // PHOTO PROFILE
    // ============================================================

    Task<AssetTrackingAttachmentDto?> GetPhotoProfileByAssetIdAsync(int assetId);

    // ============================================================
    // SCHEDULE STATS - Using GridRequest
    // ============================================================

    Task<PaginatedResult<AssetTrackingScheduleStatsDto>> GetScheduleStatsAsync(AssetTrackingGridRequest request);

    // ============================================================
    // TRANSACTION TYPE STATS
    // ============================================================

    Task<AssetTrackingTransactionTypeStatsResponse> GetTransactionTypeStatsAsync(int assetId);

    // ============================================================
    // EMPLOYEE HISTORY - Using GridRequest
    // ============================================================

    Task<PaginatedResult<AssetTrackingEmployeeHistoryDto>> GetEmployeeHistoryAsync(AssetTrackingGridRequest request);
}