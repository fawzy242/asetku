using Whitebird.Domain.Features.AssetTracking;
using Whitebird.Infra.Features.Common;

namespace Whitebird.Infra.Features.AssetTracking;

/// <summary>
/// Repository interface for Asset Tracking operations
/// </summary>
public interface IAssetTrackingReps
{
    // Lookup/Dropdown
    Task<AssetTrackingGetDataDto?> GetAssetDataAsync(int assetId);

    // Detail Tracking
    Task<AssetTrackingView?> GetTrackingDetailAsync(int assetId);

    // Stats Header
    Task<AssetStatsView?> GetAssetStatsAsync(int assetId);

    // Timeline
    Task<IEnumerable<AssetTimelineEntry>> GetTimelineAsync(int assetId);

    // Software Installation
    Task<AssetTrackingSoftwareInstallationDto?> GetSoftwareInstallationByAssetIdAsync(int assetId);

    // Attachments
    Task<IEnumerable<AssetTrackingAttachmentDto>> GetAttachmentsByAssetIdAsync(int assetId);

    // Photo Profile
    Task<AssetTrackingAttachmentDto?> GetPhotoProfileByAssetIdAsync(int assetId);

    // Schedule Stats
    Task<PaginatedResult<AssetTrackingScheduleStatsDto>> GetScheduleStatsAsync(
        int assetId, string type, int page, int pageSize, string? search = null);

    // Transaction Type Stats
    Task<AssetTrackingTransactionTypeStatsResponse> GetTransactionTypeStatsAsync(int assetId);

    // Employee History
    Task<PaginatedResult<AssetTrackingEmployeeHistoryDto>> GetEmployeeHistoryAsync(
        int assetId, int page, int pageSize, string? search = null);
}