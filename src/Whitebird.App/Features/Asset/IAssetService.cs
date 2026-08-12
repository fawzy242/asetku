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
    Task<ServiceResult<AssetDetailView>> UpdateAsync(int id, AssetUpdateViewModel model);
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
    // PHOTO PROFILE
    // ============================================================

    Task<ServiceResult> AddPhotoProfileAsync(int assetId, IFormFile file);
    Task<ServiceResult> DeletePhotoProfileAsync(int assetId);
    Task<ServiceResult<byte[]>> GetPhotoProfileAsync(int assetId);
}