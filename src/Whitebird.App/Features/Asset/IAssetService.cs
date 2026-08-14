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

    // ============================================================
    // DROPDOWN
    // ============================================================

    Task<ServiceResult<IEnumerable<AssetDropdownView>>> GetDropdownListByTransactionTypeAsync(int transactionType);

    // ============================================================
    // REMOVED METHODS (moved to internal/private):
    // - AddPhotoProfileAsync
    // - DeletePhotoProfileAsync
    // - GetPhotoProfileAsync
    // - AddAttachmentAsync
    // - GetAttachmentsAsync
    // - DeleteAttachmentAsync
    // 
    // These are now handled internally via UpdateAsync with base64 payload
    // ============================================================
}