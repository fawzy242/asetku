using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.AssetTracking;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.AssetTracking;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.AssetTracking;

/// <summary>
/// Service implementation for Asset Tracking business logic
/// </summary>
public class AssetTrackingService : BaseService, IAssetTrackingService
{
    private readonly IAssetTrackingReps _assetTrackingReps;

    public AssetTrackingService(
        IAssetTrackingReps assetTrackingReps,
        ILogger<AssetTrackingService> logger) : base(logger)
    {
        _assetTrackingReps = assetTrackingReps;
    }

    // ============================================================
    // LOOKUP
    // ============================================================

    public async Task<ServiceResult<AssetTrackingGetDataDto>> GetAssetDataAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _assetTrackingReps.GetAssetDataAsync(assetId);
            if (result == null)
            {
                return ServiceResult<AssetTrackingGetDataDto>.NotFound($"Asset with id {assetId} not found");
            }
            return ServiceResult<AssetTrackingGetDataDto>.Success(result);
        }, "get asset data for tracking");
    }

    // ============================================================
    // DETAIL TRACKING
    // ============================================================

    public async Task<ServiceResult<AssetTrackingView>> GetTrackingDetailAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _assetTrackingReps.GetTrackingDetailAsync(assetId);
            if (result == null)
            {
                return ServiceResult<AssetTrackingView>.NotFound($"Asset with id {assetId} not found");
            }
            return ServiceResult<AssetTrackingView>.Success(result);
        }, "get tracking detail");
    }

    // ============================================================
    // STATS HEADER
    // ============================================================

    public async Task<ServiceResult<AssetStatsView>> GetAssetStatsAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _assetTrackingReps.GetAssetStatsAsync(assetId);
            if (result == null)
            {
                return ServiceResult<AssetStatsView>.NotFound($"Asset with id {assetId} not found");
            }

            result.Attachments = (await _assetTrackingReps.GetAttachmentsByAssetIdAsync(assetId)).ToList();
            result.SoftwareInstallation = await _assetTrackingReps.GetSoftwareInstallationByAssetIdAsync(assetId);

            var photoProfile = await _assetTrackingReps.GetPhotoProfileByAssetIdAsync(assetId);
            result.PhotoProfileUrl = photoProfile?.FileUrl;

            return ServiceResult<AssetStatsView>.Success(result);
        }, "get asset stats");
    }

    // ============================================================
    // TIMELINE
    // ============================================================

    public async Task<ServiceResult<IEnumerable<AssetTimelineEntry>>> GetTimelineAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _assetTrackingReps.GetTimelineAsync(assetId);
            return ServiceResult<IEnumerable<AssetTimelineEntry>>.Success(result);
        }, "get asset timeline");
    }

    // ============================================================
    // SCHEDULE STATS - Using GridRequest
    // ============================================================

    public async Task<ServiceResult<PaginatedResult<AssetTrackingScheduleStatsDto>>> GetScheduleStatsAsync(AssetTrackingGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _assetTrackingReps.GetScheduleStatsAsync(request);
            return ServiceResult<PaginatedResult<AssetTrackingScheduleStatsDto>>.Success(result);
        }, "get schedule stats");
    }

    // ============================================================
    // TRANSACTION TYPE STATS
    // ============================================================

    public async Task<ServiceResult<AssetTrackingTransactionTypeStatsResponse>> GetTransactionTypeStatsAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _assetTrackingReps.GetTransactionTypeStatsAsync(assetId);
            return ServiceResult<AssetTrackingTransactionTypeStatsResponse>.Success(result);
        }, "get transaction type stats");
    }

    // ============================================================
    // EMPLOYEE HISTORY - Using GridRequest
    // ============================================================

    public async Task<ServiceResult<PaginatedResult<AssetTrackingEmployeeHistoryDto>>> GetEmployeeHistoryAsync(AssetTrackingGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _assetTrackingReps.GetEmployeeHistoryAsync(request);
            return ServiceResult<PaginatedResult<AssetTrackingEmployeeHistoryDto>>.Success(result);
        }, "get employee history");
    }
}