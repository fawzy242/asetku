using Whitebird.App.Features.Common;
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
}