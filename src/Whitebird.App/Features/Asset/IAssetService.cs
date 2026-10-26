using Microsoft.AspNetCore.Http;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.Asset;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Common;

namespace Whitebird.App.Features.Asset;

/// <summary>
/// Service interface for Asset business logic
/// </summary>
public interface IAssetService
{
    // ============================================================
    // BASIC CRUD
    // ============================================================

    Task<ServiceResult<AssetDetailView>> GetByIdAsync(int id);
    Task<ServiceResult<IEnumerable<AssetListView>>> GetAllAsync();
    Task<ServiceResult<IEnumerable<AssetListView>>> GetByCategoryAsync(int categoryId);
    Task<ServiceResult<AssetDetailView>> CreateAsync(AssetCreateViewModel model);
    Task<ServiceResult<AssetDetailView>> UpdateAsync(int id, AssetUpdateFormDto model);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> SoftDeleteAsync(int id);

    // ============================================================
    // GRID
    // ============================================================

    Task<ServiceResult<PaginatedResult<AssetListView>>> GetGridDataAsync(AssetGridRequest request);

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request);
    Task<ServiceResult<int>> BulkDeleteAsync(BulkDeleteRequest request);

    // ============================================================
    // DROPDOWN
    // ============================================================

    Task<ServiceResult<IEnumerable<AssetDropdownView>>> GetDropdownListByTransactionTypeAsync(int transactionType);

    // ============================================================
    // PHOTO & ATTACHMENTS
    // ============================================================

    /// <summary>
    /// Gets profile photo bytes with the correct Content-Type.
    /// </summary>
    Task<ServiceResult<FileContentDto>> GetPhotoAsync(int assetId);

    /// <summary>
    /// Uploads (or replaces) the profile photo from multipart form data.
    /// Returns the photo URL on success.
    /// </summary>
    Task<ServiceResult<string>> UploadPhotoAsync(int assetId, IFormFile file);

    /// <summary>
    /// Soft-deletes the current profile photo (physical file is retained).
    /// </summary>
    Task<ServiceResult> DeletePhotoAsync(int assetId);

    /// <summary>
    /// Gets attachment list for an asset.
    /// </summary>
    Task<ServiceResult<IEnumerable<AssetAttachmentViewDto>>> GetAttachmentsAsync(int assetId);

    /// <summary>
    /// Uploads a standalone attachment.
    /// </summary>
    Task<ServiceResult<AssetAttachmentViewDto>> UploadAttachmentAsync(int assetId, AttachmentUploadDto model);

    /// <summary>
    /// Soft-deletes a single attachment (physical file is retained).
    /// </summary>
    Task<ServiceResult> DeleteAttachmentAsync(int assetId, int fileAttachmentId);

    /// <summary>
    /// Streams the raw bytes of an attachment belonging to the given asset.
    /// Verifies ownership before delegating to the internal FileAttachmentService.
    /// </summary>
    Task<ServiceResult<FileContentDto>> GetAttachmentFileAsync(int assetId, int fileAttachmentId);
}