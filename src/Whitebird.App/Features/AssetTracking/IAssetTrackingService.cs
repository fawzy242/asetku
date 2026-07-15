using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.AssetTracking;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.AssetTracking;

/// <summary>
/// Service interface for Asset Tracking business logic
/// </summary>
public interface IAssetTrackingService
{
    // Lookup/Dropdown
    Task<ServiceResult<AssetTrackingGetDataDto>> GetAssetDataAsync(int assetId);

    // Detail Tracking
    Task<ServiceResult<AssetTrackingView>> GetTrackingDetailAsync(int assetId);

    // Stats Header
    Task<ServiceResult<AssetStatsView>> GetAssetStatsAsync(int assetId);

    // Timeline
    Task<ServiceResult<IEnumerable<AssetTimelineEntry>>> GetTimelineAsync(int assetId);

    // Schedule Stats
    Task<ServiceResult<PaginatedResult<AssetTrackingScheduleStatsDto>>> GetScheduleStatsAsync(
        int assetId, string type, int page, int pageSize, string? search = null);

    // Transaction Type Stats
    Task<ServiceResult<AssetTrackingTransactionTypeStatsResponse>> GetTransactionTypeStatsAsync(int assetId);

    // Employee History
    Task<ServiceResult<PaginatedResult<AssetTrackingEmployeeHistoryDto>>> GetEmployeeHistoryAsync(
        int assetId, int page, int pageSize, string? search = null);
}