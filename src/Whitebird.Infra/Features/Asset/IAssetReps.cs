using Whitebird.Domain.Features.Asset;
using Whitebird.Domain.Features.Common;

namespace Whitebird.Infra.Features.Asset;

/// <summary>
/// Repository interface for Asset operations
/// </summary>
public interface IAssetReps
{
    // ============================================================
    // RAW ENTITY METHODS
    // ============================================================

    Task<AssetEntity?> GetByIdRawAsync(int assetId);
    Task<bool> IsAssetCodeExistsAsync(string assetCode, int? excludeAssetId = null);

    /// <summary>
    /// Counts AssetTransaction rows pointing at this asset. Hard-delete FK guard.
    /// </summary>
    Task<int> GetAssetTransactionCountAsync(int assetId);

    /// <summary>
    /// Counts ScheduleHandover rows pointing at this asset. Hard-delete FK guard.
    /// </summary>
    Task<int> GetScheduleHandoverCountAsync(int assetId);

    /// <summary>
    /// Counts ScheduleReturn rows pointing at this asset. Hard-delete FK guard.
    /// </summary>
    Task<int> GetScheduleReturnCountAsync(int assetId);

    /// <summary>
    /// Counts ScheduleMaintenance rows pointing at this asset. Hard-delete FK guard.
    /// </summary>
    Task<int> GetScheduleMaintenanceCountAsync(int assetId);

    // ============================================================
    // DETAIL VIEW METHODS
    // ============================================================

    Task<AssetDetailView?> GetDetailByIdAsync(int assetId);
    Task<IEnumerable<AssetListView>> GetAllListViewAsync();
    Task<IEnumerable<AssetListView>> GetByCategoryListViewAsync(int categoryId);

    // ============================================================
    // PAGINATION
    // ============================================================

    Task<PaginatedResult<AssetListView>> GetPagedListAsync(
        int page, int pageSize, string? search = null, string? sortBy = null,
        bool sortDescending = false, Dictionary<string, object>? filters = null);

    // ============================================================
    // DROPDOWN
    // ============================================================

    Task<IEnumerable<AssetDropdownView>> GetDropdownListByTransactionTypeAsync(int transactionType);

    // ============================================================
    // SOFTWARE INSTALLATION
    // ============================================================

    Task<SoftwareInstallationEntity?> GetSoftwareInstallationByAssetIdAsync(int assetId);
    Task<int> InsertSoftwareInstallationAsync(SoftwareInstallationEntity entity);
    Task<int> UpdateSoftwareInstallationAsync(SoftwareInstallationEntity entity);
    Task<int> DeleteSoftwareInstallationByAssetIdAsync(int assetId);

    // ============================================================
    // ASSET ATTACHMENT
    // ============================================================

    Task<int> InsertAttachmentAsync(AssetAttachmentEntity entity);
    Task<IEnumerable<AssetAttachmentEntity>> GetAttachmentsByAssetIdAsync(int assetId);
    Task<AssetAttachmentEntity?> GetAttachmentByIdAsync(int attachmentId);
    Task<int> DeleteAttachmentAsync(int attachmentId);
    Task<int> DeleteAttachmentsByAssetIdAsync(int assetId);

    // ============================================================
    // ASSET PHOTO PROFILE
    // ============================================================

    Task<int> InsertPhotoProfileAsync(AssetPhotoProfileAttachmentEntity entity);
    Task<AssetPhotoProfileAttachmentEntity?> GetPhotoProfileByAssetIdAsync(int assetId);
    Task<int> DeletePhotoProfileByAssetIdAsync(int assetId);
}