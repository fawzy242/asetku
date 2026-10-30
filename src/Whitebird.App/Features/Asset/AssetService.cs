using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.FileAttachment;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Asset;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.Asset;
using Whitebird.Infra.Features.Category;
using Whitebird.Infra.Features.Common;
using Whitebird.Infra.Features.Supplier;

namespace Whitebird.App.Features.Asset;

public class AssetService : BaseService, IAssetService
{
    private readonly ICommonReps _repository;
    private readonly IAssetReps _assetReps;
    private readonly ICategoryReps _categoryReps;
    private readonly ISupplierReps _supplierReps;
    private readonly IMasterDataService _masterDataService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;
    private readonly IStorageService _storageService;
    private readonly IFileAttachmentService _fileAttachmentService;

    public AssetService(
        ICommonReps repository,
        IAssetReps assetReps,
        ICategoryReps categoryReps,
        ISupplierReps supplierReps,
        IMasterDataService masterDataService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        IStorageService storageService,
        IFileAttachmentService fileAttachmentService,
        ILogger<AssetService> logger) : base(logger)
    {
        _repository = repository;
        _assetReps = assetReps;
        _categoryReps = categoryReps;
        _supplierReps = supplierReps;
        _masterDataService = masterDataService;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
        _storageService = storageService;
        _fileAttachmentService = fileAttachmentService;
    }

    // ============================================================
    // BASIC CRUD
    // ============================================================

    public async Task<ServiceResult<AssetDetailView>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetDetailByIdAsync(id);
            if (asset == null)
                return ServiceResult<AssetDetailView>.NotFound($"Asset with id {id} not found");

            var software = await _assetReps.GetSoftwareInstallationByAssetIdAsync(id);
            if (software != null)
            {
                asset.SoftwareInstallation = software.Adapt<SoftwareInstallationView>();

                if (software.OperatingSystem.HasValue)
                {
                    var os = await _masterDataService.GetValueAsync("OperatingSystem", software.OperatingSystem.Value);
                    if (os.IsSuccess) asset.SoftwareInstallation.OperatingSystemName = os.Data;
                }
                if (software.MicrosoftOffice.HasValue)
                {
                    var office = await _masterDataService.GetValueAsync("MicrosoftOffice", software.MicrosoftOffice.Value);
                    if (office.IsSuccess) asset.SoftwareInstallation.MicrosoftOfficeName = office.Data;
                }
                if (software.VisualStudio.HasValue)
                {
                    var vs = await _masterDataService.GetValueAsync("VisualStudio", software.VisualStudio.Value);
                    if (vs.IsSuccess) asset.SoftwareInstallation.VisualStudioName = vs.Data;
                }
                if (software.DBMS.HasValue)
                {
                    var dbms = await _masterDataService.GetValueAsync("DBMS", software.DBMS.Value);
                    if (dbms.IsSuccess) asset.SoftwareInstallation.DBMSName = dbms.Data;
                }
            }

            return ServiceResult<AssetDetailView>.Success(asset);
        }, "get asset by id");
    }

    public async Task<ServiceResult<IEnumerable<AssetListView>>> GetAllAsync()
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assets = await _assetReps.GetAllListViewAsync();
            return ServiceResult<IEnumerable<AssetListView>>.Success(assets);
        }, "get all assets");
    }

    public async Task<ServiceResult<IEnumerable<AssetListView>>> GetByCategoryAsync(int categoryId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var category = await _categoryReps.GetByIdRawAsync(categoryId);
            if (category == null)
                return ServiceResult<IEnumerable<AssetListView>>.NotFound($"Category with id {categoryId} not found");

            var assets = await _assetReps.GetByCategoryListViewAsync(categoryId);
            return ServiceResult<IEnumerable<AssetListView>>.Success(assets);
        }, "get assets by category");
    }

    public async Task<ServiceResult<AssetDetailView>> CreateAsync(AssetCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.AssetCode))
            return ServiceResult<AssetDetailView>.BadRequest("Asset code is required");
        if (string.IsNullOrWhiteSpace(model.AssetName))
            return ServiceResult<AssetDetailView>.BadRequest("Asset name is required");
        if (model.CategoryId <= 0)
            return ServiceResult<AssetDetailView>.BadRequest("Valid category is required");

        if (await _assetReps.IsAssetCodeExistsAsync(model.AssetCode))
            return ServiceResult<AssetDetailView>.Conflict($"Asset code '{model.AssetCode}' already exists");

        if (await _categoryReps.GetByIdRawAsync(model.CategoryId) == null)
            return ServiceResult<AssetDetailView>.BadRequest($"Category with id {model.CategoryId} does not exist");

        if (model.SupplierId.HasValue && model.SupplierId.Value > 0)
        {
            if (await _supplierReps.GetByIdRawAsync(model.SupplierId.Value) == null)
                return ServiceResult<AssetDetailView>.BadRequest($"Supplier with id {model.SupplierId} does not exist");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var entity = model.Adapt<AssetEntity>();
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            entity.CreatedBy = _currentUserService.GetDisplayName();

            var id = await _repository.InsertAsync(entity);
            var assetId = Convert.ToInt32(id);

            if (model.SoftwareInstallation != null)
            {
                var softwareEntity = model.SoftwareInstallation.Adapt<SoftwareInstallationEntity>();
                softwareEntity.AssetId = assetId;
                softwareEntity.CreatedBy = _currentUserService.GetDisplayName();
                softwareEntity.CreatedDate = DateTime.Now;
                softwareEntity.IsActive = true;
                await _assetReps.InsertSoftwareInstallationAsync(softwareEntity);
            }

            var created = await _assetReps.GetDetailByIdAsync(assetId);

            if (created != null)
            {
                await _activityLogService.LogCreateAsync(
                    TableNames.Asset, created.AssetId,
                    $"Asset '{created.AssetCode}' - '{created.AssetName}' created successfully",
                    _currentUserService.GetDisplayName());
            }

            return created == null
                ? ServiceResult<AssetDetailView>.Failure("Failed to retrieve created asset")
                : ServiceResult<AssetDetailView>.Success(created, "Asset created successfully");
        }, "create asset");
    }

    public async Task<ServiceResult<AssetDetailView>> UpdateAsync(int id, AssetUpdateFormDto model)
    {
        if (string.IsNullOrWhiteSpace(model.AssetCode))
            return ServiceResult<AssetDetailView>.BadRequest("Asset code is required");
        if (string.IsNullOrWhiteSpace(model.AssetName))
            return ServiceResult<AssetDetailView>.BadRequest("Asset name is required");
        if (model.CategoryId <= 0)
            return ServiceResult<AssetDetailView>.BadRequest("Valid category is required");

        if (await _assetReps.IsAssetCodeExistsAsync(model.AssetCode, id))
            return ServiceResult<AssetDetailView>.Conflict($"Asset code '{model.AssetCode}' already exists");

        if (await _categoryReps.GetByIdRawAsync(model.CategoryId) == null)
            return ServiceResult<AssetDetailView>.BadRequest($"Category with id {model.CategoryId} does not exist");

        if (model.SupplierId.HasValue && model.SupplierId.Value > 0)
        {
            if (await _supplierReps.GetByIdRawAsync(model.SupplierId.Value) == null)
                return ServiceResult<AssetDetailView>.BadRequest($"Supplier with id {model.SupplierId} does not exist");
        }

        if (model.AssetConditionPurchase.HasValue)
        {
            var cond = await _masterDataService.GetValueAsync("AssetConditionPurchase", model.AssetConditionPurchase.Value);
            if (!cond.IsSuccess || cond.Data == null)
                return ServiceResult<AssetDetailView>.BadRequest($"Invalid AssetConditionPurchase: {model.AssetConditionPurchase}");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _assetReps.GetByIdRawAsync(id);
            if (existing == null) return ServiceResult<AssetDetailView>.NotFound($"Asset with id {id} not found");

            existing.AssetCode = model.AssetCode;
            existing.AssetName = model.AssetName;
            existing.CategoryId = model.CategoryId;
            existing.Brand = model.Brand;
            existing.Model = model.Model;
            existing.SerialNumber = model.SerialNumber;
            existing.Imei = model.Imei;
            existing.MacAddress = model.MacAddress;
            existing.Hostname = model.Hostname;
            existing.IpAddress = model.IpAddress;
            existing.PurchaseDate = model.PurchaseDate;
            existing.PurchasePrice = model.PurchasePrice;
            existing.InvoiceNumber = model.InvoiceNumber;
            existing.SupplierId = model.SupplierId;
            existing.WarrantyPeriod = model.WarrantyPeriod;
            existing.AssetConditionPurchase = model.AssetConditionPurchase;
            existing.Notes = model.Notes;
            existing.IsActive = model.IsActive;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            await _repository.UpdateAsync(existing);

            if (model.SoftwareInstallation != null)
            {
                var existingSoftware = await _assetReps.GetSoftwareInstallationByAssetIdAsync(id);
                if (existingSoftware != null)
                {
                    model.SoftwareInstallation.Adapt(existingSoftware);
                    existingSoftware.ModifiedDate = DateTime.Now;
                    existingSoftware.ModifiedBy = _currentUserService.GetDisplayName();
                    await _assetReps.UpdateSoftwareInstallationAsync(existingSoftware);
                }
                else
                {
                    var softwareEntity = model.SoftwareInstallation.Adapt<SoftwareInstallationEntity>();
                    softwareEntity.AssetId = id;
                    softwareEntity.CreatedBy = _currentUserService.GetDisplayName();
                    softwareEntity.CreatedDate = DateTime.Now;
                    softwareEntity.IsActive = true;
                    await _assetReps.InsertSoftwareInstallationAsync(softwareEntity);
                }
            }

            if (!string.IsNullOrEmpty(model.PhotoFileBase64))
            {
                var oldPhoto = await _assetReps.GetPhotoProfileByAssetIdAsync(id);
                if (oldPhoto != null)
                {
                    await _assetReps.DeletePhotoProfileByAssetIdAsync(id);
                    await _fileAttachmentService.DeleteAsync(oldPhoto.FileAttachmentId);
                }
                await AddPhotoProfileFromBase64Async(id, model.PhotoFileBase64, model.PhotoFileName ?? "photo.jpg");
            }

            if (model.ReplaceAttachments || (model.AttachmentFilesBase64 != null && model.AttachmentFilesBase64.Any()))
            {
                var existingAttachments = await _assetReps.GetAttachmentsByAssetIdAsync(id);
                var keepIds = model.AttachmentIdsToKeep ?? new List<int>();

                foreach (var att in existingAttachments)
                {
                    if (!keepIds.Contains(att.FileAttachmentId))
                    {
                        await _assetReps.DeleteAttachmentAsync(att.AssetAttachmentId);
                        await _fileAttachmentService.DeleteAsync(att.FileAttachmentId);
                    }
                }

                if (model.AttachmentFilesBase64 != null && model.AttachmentFilesBase64.Any())
                {
                    int order = existingAttachments.Count(a => keepIds.Contains(a.FileAttachmentId));
                    for (int i = 0; i < model.AttachmentFilesBase64.Count; i++)
                    {
                        var base64 = model.AttachmentFilesBase64[i];
                        var fileName = model.AttachmentFileNames?[i] ?? $"attachment_{i}.pdf";
                        if (!string.IsNullOrEmpty(base64))
                        {
                            await AddAttachmentFromBase64Async(id, base64, fileName, order);
                            order++;
                        }
                    }
                }
            }

            var updated = await _assetReps.GetDetailByIdAsync(id);

            await _activityLogService.LogUpdateAsync(TableNames.Asset, id,
                $"Asset updated: Code '{model.AssetCode}', Name '{model.AssetName}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetDetailView>.Success(updated!, "Asset updated successfully");
        }, "update asset");
    }

    // ============================================================
    // DELETE
    // ============================================================

    /// <summary>
    /// Determines whether the asset can be hard-deleted. Blocks on
    /// AssetTransaction and Schedule* dependents (business records).
    /// Attachments / photo profile / software installation are cascaded
    /// automatically by <see cref="CascadeDeleteDependentsAsync"/>.
    /// </summary>
    private async Task<string?> GetDeleteBlockReasonAsync(int id)
    {
        var transactionCount = await _assetReps.GetAssetTransactionCountAsync(id);
        if (transactionCount > 0)
            return $"Cannot delete asset with {transactionCount} associated transaction(s)";

        var handoverCount = await _assetReps.GetScheduleHandoverCountAsync(id);
        if (handoverCount > 0)
            return $"Cannot delete asset with {handoverCount} associated handover schedule(s)";

        var returnCount = await _assetReps.GetScheduleReturnCountAsync(id);
        if (returnCount > 0)
            return $"Cannot delete asset with {returnCount} associated return schedule(s)";

        var maintenanceCount = await _assetReps.GetScheduleMaintenanceCountAsync(id);
        if (maintenanceCount > 0)
            return $"Cannot delete asset with {maintenanceCount} associated maintenance schedule(s)";

        return null;
    }

    /// <summary>
    /// Hard-deletes the asset's dependent rows that are otherwise inaccessible
    /// via the API (attachments, photo profile, software installation).
    /// FileAttachment metadata is soft-deleted first (matching the pattern used
    /// during UpdateAsync attachment removal), then the join row is hard-deleted
    /// so the FK on the asset is cleared. Runs inside the caller's transaction.
    /// </summary>
    private async Task CascadeDeleteDependentsAsync(int id)
    {
        // Asset attachments
        var attachments = await _assetReps.GetAttachmentsByAssetIdAsync(id);
        foreach (var att in attachments)
        {
            await _fileAttachmentService.DeleteAsync(att.FileAttachmentId);
            await _repository.DeleteAsync<AssetAttachmentEntity>(att.AssetAttachmentId);
        }

        // Asset photo profile (0 or 1)
        var photo = await _assetReps.GetPhotoProfileByAssetIdAsync(id);
        if (photo != null)
        {
            await _fileAttachmentService.DeleteAsync(photo.FileAttachmentId);
            await _repository.DeleteAsync<AssetPhotoProfileAttachmentEntity>(photo.AssetPhotoProfileAttachmentId);
        }

        // Software installation (0 or 1 due to UNIQUE(AssetId))
        var software = await _assetReps.GetSoftwareInstallationByAssetIdAsync(id);
        if (software != null)
        {
            await _repository.DeleteAsync<SoftwareInstallationEntity>(software.SoftwareInstallationId);
        }
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _assetReps.GetByIdRawAsync(id);
            if (existing == null) return ServiceResult.NotFound($"Asset with id {id} not found");

            var blockReason = await GetDeleteBlockReasonAsync(id);
            if (blockReason != null)
                return ServiceResult.BadRequest(blockReason);

            await CascadeDeleteDependentsAsync(id);

            var result = await _repository.DeleteAsync<AssetEntity>(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(TableNames.Asset, id,
                    $"Asset '{existing.AssetCode}' - '{existing.AssetName}' deleted permanently",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to delete asset")
                : ServiceResult.Success("Asset deleted successfully");
        }, "delete asset");
    }

    public async Task<ServiceResult> SoftDeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _assetReps.GetByIdRawAsync(id);
            if (existing == null) return ServiceResult.NotFound($"Asset with id {id} not found");

            await _assetReps.DeleteSoftwareInstallationByAssetIdAsync(id);

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogSoftDeleteAsync(TableNames.Asset, id,
                    $"Asset '{existing.AssetCode}' - '{existing.AssetName}' soft deleted",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to soft delete asset")
                : ServiceResult.Success("Asset soft deleted successfully");
        }, "soft delete asset");
    }

    // ============================================================
    // GRID (P3-1 — request passed straight through)
    // ============================================================

    public async Task<ServiceResult<PaginatedResult<AssetListView>>> GetGridDataAsync(AssetGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var result = await _assetReps.GetPagedListAsync(request);
            return ServiceResult<PaginatedResult<AssetListView>>.Success(result);
        }, "get asset grid data");
    }

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    public async Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
            return ServiceResult<int>.BadRequest("No asset IDs provided");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var count = 0;
            foreach (var id in request.Ids)
            {
                var asset = await _assetReps.GetByIdRawAsync(id);
                if (asset != null && !asset.IsActive)
                {
                    asset.IsActive = true;
                    asset.ModifiedDate = DateTime.Now;
                    asset.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(asset);
                    count++;
                    await _activityLogService.LogUpdateAsync(TableNames.Asset, id,
                        $"Asset '{asset.AssetCode}' activated via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }
            return ServiceResult<int>.Success(count, $"{count} asset(s) activated successfully");
        }, "bulk activate assets");
    }

    public async Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
            return ServiceResult<int>.BadRequest("No asset IDs provided");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var count = 0;
            foreach (var id in request.Ids)
            {
                var asset = await _assetReps.GetByIdRawAsync(id);
                if (asset != null && asset.IsActive)
                {
                    asset.IsActive = false;
                    asset.ModifiedDate = DateTime.Now;
                    asset.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(asset);
                    count++;
                    await _activityLogService.LogUpdateAsync(TableNames.Asset, id,
                        $"Asset '{asset.AssetCode}' deactivated via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }
            return ServiceResult<int>.Success(count, $"{count} asset(s) deactivated successfully");
        }, "bulk deactivate assets");
    }

    public async Task<ServiceResult<int>> BulkDeleteAsync(BulkDeleteRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
            return ServiceResult<int>.BadRequest("No asset IDs provided");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var count = 0;
            foreach (var id in request.Ids)
            {
                var existing = await _assetReps.GetByIdRawAsync(id);
                if (existing == null) continue;

                var blockReason = await GetDeleteBlockReasonAsync(id);
                if (blockReason != null) continue;

                await CascadeDeleteDependentsAsync(id);

                var result = await _repository.DeleteAsync<AssetEntity>(id);
                if (result > 0)
                {
                    count++;
                    await _activityLogService.LogDeleteAsync(TableNames.Asset, id,
                        $"Asset '{existing.AssetCode}' - '{existing.AssetName}' deleted via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }
            var message = count > 0
                ? $"{count} asset(s) deleted successfully"
                : "No assets were deleted. Assets with associated transactions or schedules cannot be deleted.";
            return ServiceResult<int>.Success(count, message);
        }, "bulk delete assets");
    }

    public async Task<ServiceResult<IEnumerable<AssetDropdownView>>> GetDropdownListByTransactionTypeAsync(int transactionType)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assets = await _assetReps.GetDropdownListByTransactionTypeAsync(transactionType);
            return ServiceResult<IEnumerable<AssetDropdownView>>.Success(assets);
        }, "get asset dropdown by transaction type");
    }

    // ============================================================
    // PHOTO
    // ============================================================

    public async Task<ServiceResult<FileContentDto>> GetPhotoAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetByIdRawAsync(assetId);
            if (asset == null) return ServiceResult<FileContentDto>.NotFound($"Asset with id {assetId} not found");

            var photo = await _assetReps.GetPhotoProfileByAssetIdAsync(assetId);
            if (photo == null) return ServiceResult<FileContentDto>.NotFound("Profile photo not found");

            var fileResult = await _fileAttachmentService.GetByIdAsync(photo.FileAttachmentId);
            if (!fileResult.IsSuccess || fileResult.Data == null)
                return ServiceResult<FileContentDto>.NotFound("Profile photo file not found");

            var fileBytes = await _storageService.ReadFileAsync(fileResult.Data.FilePath);
            var contentType = _storageService.GetMimeType(fileResult.Data.OriginalFileName);

            return ServiceResult<FileContentDto>.Success(new FileContentDto
            {
                Bytes = fileBytes,
                ContentType = contentType,
                FileName = fileResult.Data.OriginalFileName
            });
        }, "get asset photo");
    }

    public async Task<ServiceResult<string>> UploadPhotoAsync(int assetId, IFormFile file)
    {
        var (isValid, error) = FileUploadValidator.ValidateProfilePhoto(file);
        if (!isValid) return ServiceResult<string>.BadRequest(error!);

        var asset = await _assetReps.GetByIdRawAsync(assetId);
        if (asset == null) return ServiceResult<string>.NotFound($"Asset with id {assetId} not found");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var oldPhoto = await _assetReps.GetPhotoProfileByAssetIdAsync(assetId);
            if (oldPhoto != null)
            {
                await _assetReps.DeletePhotoProfileByAssetIdAsync(assetId);
                await _fileAttachmentService.DeleteAsync(oldPhoto.FileAttachmentId);
            }

            using var stream = file!.OpenReadStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var uploadResult = await _fileAttachmentService.UploadBytesAsync(
                referenceTable: "AssetPhotoProfile", referenceId: assetId,
                originalFileName: file.FileName, bytes: bytes,
                fileCategory: "ProfilePhoto", description: $"Profile photo for Asset {assetId}",
                isPrimary: true);

            if (!uploadResult.IsSuccess || uploadResult.Data == null)
                return ServiceResult<string>.Failure(uploadResult.Errors);

            var entity = new AssetPhotoProfileAttachmentEntity
            {
                AssetId = assetId,
                FileAttachmentId = uploadResult.Data.FileAttachmentId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = _currentUserService.GetDisplayName()
            };
            await _assetReps.InsertPhotoProfileAsync(entity);

            await _activityLogService.LogUpdateAsync(TableNames.Asset, assetId,
                "Profile photo uploaded", _currentUserService.GetDisplayName());

            return ServiceResult<string>.Success(uploadResult.Data.FileUrl ?? string.Empty, "Profile photo uploaded successfully");
        }, "upload asset photo");
    }

    public async Task<ServiceResult> DeletePhotoAsync(int assetId)
    {
        var asset = await _assetReps.GetByIdRawAsync(assetId);
        if (asset == null) return ServiceResult.NotFound($"Asset with id {assetId} not found");

        return await ExecuteWithTransactionAsync(async () =>
        {
            var photo = await _assetReps.GetPhotoProfileByAssetIdAsync(assetId);
            if (photo == null) return ServiceResult.Success("No profile photo to delete");

            await _assetReps.DeletePhotoProfileByAssetIdAsync(assetId);
            await _fileAttachmentService.DeleteAsync(photo.FileAttachmentId);

            await _activityLogService.LogUpdateAsync(TableNames.Asset, assetId,
                "Profile photo deleted", _currentUserService.GetDisplayName());

            return ServiceResult.Success("Profile photo deleted successfully");
        }, "delete asset photo");
    }

    // ============================================================
    // ATTACHMENTS
    // ============================================================

    public async Task<ServiceResult<IEnumerable<AssetAttachmentViewDto>>> GetAttachmentsAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var detail = await _assetReps.GetDetailByIdAsync(assetId);
            if (detail == null)
                return ServiceResult<IEnumerable<AssetAttachmentViewDto>>.NotFound($"Asset with id {assetId} not found");
            return ServiceResult<IEnumerable<AssetAttachmentViewDto>>.Success(detail.Attachments);
        }, "get asset attachments");
    }

    public async Task<ServiceResult<AssetAttachmentViewDto>> UploadAttachmentAsync(int assetId, AttachmentUploadDto model)
    {
        var (isValid, error) = FileUploadValidator.ValidateAssetAttachment(model.File);
        if (!isValid) return ServiceResult<AssetAttachmentViewDto>.BadRequest(error!);

        var asset = await _assetReps.GetByIdRawAsync(assetId);
        if (asset == null) return ServiceResult<AssetAttachmentViewDto>.NotFound($"Asset with id {assetId} not found");

        return await ExecuteWithTransactionAsync(async () =>
        {
            using var stream = model.File!.OpenReadStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var uploadResult = await _fileAttachmentService.UploadBytesAsync(
                referenceTable: "Asset", referenceId: assetId,
                originalFileName: model.File.FileName, bytes: bytes,
                fileCategory: model.FileCategory ?? "AssetAttachment",
                description: model.Description ?? $"Attachment for Asset {assetId}",
                isPrimary: false);

            if (!uploadResult.IsSuccess || uploadResult.Data == null)
                return ServiceResult<AssetAttachmentViewDto>.Failure(uploadResult.Errors);

            var attachmentEntity = new AssetAttachmentEntity
            {
                AssetId = assetId,
                FileAttachmentId = uploadResult.Data.FileAttachmentId,
                Order = model.Order,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = _currentUserService.GetDisplayName()
            };
            var attachmentId = await _assetReps.InsertAttachmentAsync(attachmentEntity);

            await _activityLogService.LogCreateAsync(TableNames.Asset, assetId,
                $"Attachment uploaded: {model.File.FileName}",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetAttachmentViewDto>.Success(new AssetAttachmentViewDto
            {
                AssetAttachmentId = attachmentId,
                FileAttachmentId = uploadResult.Data.FileAttachmentId,
                FileName = uploadResult.Data.FileName,
                OriginalFileName = uploadResult.Data.OriginalFileName,
                FileUrl = uploadResult.Data.FileUrl,
                FileCategory = uploadResult.Data.FileCategory,
                IsPrimary = false,
                Order = model.Order,
                CreatedDate = DateTime.Now
            }, "Attachment uploaded successfully");
        }, "upload asset attachment");
    }

    public async Task<ServiceResult> DeleteAttachmentAsync(int assetId, int fileAttachmentId)
    {
        var asset = await _assetReps.GetByIdRawAsync(assetId);
        if (asset == null) return ServiceResult.NotFound($"Asset with id {assetId} not found");

        var attachments = await _assetReps.GetAttachmentsByAssetIdAsync(assetId);
        var target = attachments.FirstOrDefault(a => a.FileAttachmentId == fileAttachmentId);
        if (target == null) return ServiceResult.NotFound($"Attachment {fileAttachmentId} not found for asset {assetId}");

        return await ExecuteWithTransactionAsync(async () =>
        {
            await _assetReps.DeleteAttachmentAsync(target.AssetAttachmentId);
            await _fileAttachmentService.DeleteAsync(fileAttachmentId);

            await _activityLogService.LogDeleteAsync(TableNames.Asset, assetId,
                $"Attachment deleted: {fileAttachmentId}",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Attachment deleted successfully");
        }, "delete asset attachment");
    }

    public async Task<ServiceResult<FileContentDto>> GetAttachmentFileAsync(int assetId, int fileAttachmentId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetByIdRawAsync(assetId);
            if (asset == null) return ServiceResult<FileContentDto>.NotFound($"Asset with id {assetId} not found");

            var attachments = await _assetReps.GetAttachmentsByAssetIdAsync(assetId);
            if (!attachments.Any(a => a.FileAttachmentId == fileAttachmentId))
                return ServiceResult<FileContentDto>.NotFound($"Attachment {fileAttachmentId} not found for asset {assetId}");

            var downloadResult = await _fileAttachmentService.DownloadAsync(fileAttachmentId);
            if (!downloadResult.IsSuccess || downloadResult.Data == null)
                return ServiceResult<FileContentDto>.NotFound($"Attachment file {fileAttachmentId} could not be retrieved");

            return downloadResult;
        }, "get asset attachment file");
    }

    // ============================================================
    // PRIVATE HELPERS — BASE64 UPLOAD
    // ============================================================

    private async Task AddPhotoProfileFromBase64Async(int assetId, string base64String, string fileName)
    {
        var bytes = Convert.FromBase64String(base64String);
        var result = await _fileAttachmentService.UploadBytesAsync(
            referenceTable: "AssetPhotoProfile", referenceId: assetId,
            originalFileName: fileName, bytes: bytes,
            fileCategory: "ProfilePhoto", description: $"Profile photo for Asset {assetId}",
            isPrimary: true);

        if (!result.IsSuccess || result.Data == null)
            throw new Exception($"Upload profile photo failed: {string.Join(", ", result.Errors)}");

        var entity = new AssetPhotoProfileAttachmentEntity
        {
            AssetId = assetId,
            FileAttachmentId = result.Data.FileAttachmentId,
            IsActive = true,
            CreatedDate = DateTime.Now,
            CreatedBy = _currentUserService.GetDisplayName()
        };
        await _assetReps.InsertPhotoProfileAsync(entity);

        await _activityLogService.LogUpdateAsync(TableNames.Asset, assetId,
            "Profile photo uploaded", _currentUserService.GetDisplayName());
    }

    private async Task AddAttachmentFromBase64Async(int assetId, string base64String, string fileName, int order)
    {
        var bytes = Convert.FromBase64String(base64String);
        var fileResult = await _fileAttachmentService.UploadBytesAsync(
            referenceTable: "Asset", referenceId: assetId,
            originalFileName: fileName, bytes: bytes,
            fileCategory: "AssetAttachment",
            description: $"Attachment for Asset {assetId}",
            isPrimary: false);

        if (!fileResult.IsSuccess || fileResult.Data == null)
            throw new Exception($"Upload attachment failed: {string.Join(", ", fileResult.Errors)}");

        var attachmentEntity = new AssetAttachmentEntity
        {
            AssetId = assetId,
            FileAttachmentId = fileResult.Data.FileAttachmentId,
            Order = order,
            IsActive = true,
            CreatedDate = DateTime.Now,
            CreatedBy = _currentUserService.GetDisplayName()
        };
        await _assetReps.InsertAttachmentAsync(attachmentEntity);

        await _activityLogService.LogCreateAsync(TableNames.Asset, assetId,
            $"Attachment added: {fileName}",
            _currentUserService.GetDisplayName());
    }
}