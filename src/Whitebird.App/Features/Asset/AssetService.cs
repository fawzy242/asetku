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

/// <summary>
/// Service implementation for Asset business logic
/// </summary>
public class AssetService : BaseService, IAssetService
{
    private readonly IGenericRepository<AssetEntity> _repository;
    private readonly IAssetReps _assetReps;
    private readonly ICategoryReps _categoryReps;
    private readonly ISupplierReps _supplierReps;
    private readonly IMasterDataService _masterDataService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;
    private readonly IStorageService _storageService;
    private readonly IFileAttachmentService _fileAttachmentService;

    public AssetService(
        IGenericRepository<AssetEntity> repository,
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
            {
                return ServiceResult<AssetDetailView>.NotFound($"Asset with id {id} not found");
            }

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
            {
                return ServiceResult<IEnumerable<AssetListView>>.NotFound($"Category with id {categoryId} not found");
            }

            var assets = await _assetReps.GetByCategoryListViewAsync(categoryId);
            return ServiceResult<IEnumerable<AssetListView>>.Success(assets);
        }, "get assets by category");
    }

    public async Task<ServiceResult<AssetDetailView>> CreateAsync(AssetCreateViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.AssetCode))
        {
            return ServiceResult<AssetDetailView>.BadRequest("Asset code is required");
        }

        if (string.IsNullOrWhiteSpace(model.AssetName))
        {
            return ServiceResult<AssetDetailView>.BadRequest("Asset name is required");
        }

        if (model.CategoryId <= 0)
        {
            return ServiceResult<AssetDetailView>.BadRequest("Valid category is required");
        }

        var codeExists = await _assetReps.IsAssetCodeExistsAsync(model.AssetCode);
        if (codeExists)
        {
            return ServiceResult<AssetDetailView>.Conflict($"Asset code '{model.AssetCode}' already exists");
        }

        var category = await _categoryReps.GetByIdRawAsync(model.CategoryId);
        if (category == null)
        {
            return ServiceResult<AssetDetailView>.BadRequest($"Category with id {model.CategoryId} does not exist");
        }

        if (model.SupplierId.HasValue && model.SupplierId.Value > 0)
        {
            var supplier = await _supplierReps.GetByIdRawAsync(model.SupplierId.Value);
            if (supplier == null)
            {
                return ServiceResult<AssetDetailView>.BadRequest($"Supplier with id {model.SupplierId} does not exist");
            }
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
                    TableNames.Asset,
                    created.AssetId,
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
        var codeExists = await _assetReps.IsAssetCodeExistsAsync(model.AssetCode, id);
        if (codeExists)
        {
            return ServiceResult<AssetDetailView>.Conflict($"Asset code '{model.AssetCode}' already exists");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _assetReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult<AssetDetailView>.NotFound($"Asset with id {id} not found");
            }

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

            await _activityLogService.LogUpdateAsync(
                TableNames.Asset,
                id,
                $"Asset updated: Code '{model.AssetCode}', Name '{model.AssetName}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetDetailView>.Success(updated!, "Asset updated successfully");
        }, "update asset");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _assetReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Asset with id {id} not found");
            }

            await _assetReps.DeleteSoftwareInstallationByAssetIdAsync(id);
            var result = await _repository.DeleteAsync(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(
                    TableNames.Asset,
                    id,
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
            if (existing == null)
            {
                return ServiceResult.NotFound($"Asset with id {id} not found");
            }

            await _assetReps.DeleteSoftwareInstallationByAssetIdAsync(id);

            existing.IsActive = false;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _repository.UpdateAsync(existing);

            if (result > 0)
            {
                await _activityLogService.LogSoftDeleteAsync(
                    TableNames.Asset,
                    id,
                    $"Asset '{existing.AssetCode}' - '{existing.AssetName}' soft deleted",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to soft delete asset")
                : ServiceResult.Success("Asset soft deleted successfully");
        }, "soft delete asset");
    }

    // ============================================================
    // GRID
    // ============================================================

    public async Task<ServiceResult<PaginatedResult<AssetListView>>> GetGridDataAsync(AssetGridRequest request)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var filters = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(request.Status)) filters["status"] = request.Status;
            if (request.CategoryId.HasValue) filters["categoryId"] = request.CategoryId.Value;
            if (request.SupplierId.HasValue) filters["supplierId"] = request.SupplierId.Value;
            if (request.IsActive.HasValue) filters["isActive"] = request.IsActive.Value;

            var result = await _assetReps.GetPagedListAsync(
                request.Page,
                request.PageSize,
                request.Search,
                request.SortBy,
                request.SortDescending,
                filters);

            return ServiceResult<PaginatedResult<AssetListView>>.Success(result);
        }, "get asset grid data");
    }

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    public async Task<ServiceResult<int>> BulkActivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No asset IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var activatedCount = 0;

            foreach (var id in request.Ids)
            {
                var asset = await _assetReps.GetByIdRawAsync(id);
                if (asset != null && !asset.IsActive)
                {
                    asset.IsActive = true;
                    asset.ModifiedDate = DateTime.Now;
                    asset.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(asset);
                    activatedCount++;

                    await _activityLogService.LogUpdateAsync(
                        TableNames.Asset,
                        id,
                        $"Asset '{asset.AssetCode}' activated via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            return ServiceResult<int>.Success(activatedCount,
                $"{activatedCount} asset(s) activated successfully");
        }, "bulk activate assets");
    }

    public async Task<ServiceResult<int>> BulkDeactivateAsync(BulkActivateRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No asset IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var deactivatedCount = 0;

            foreach (var id in request.Ids)
            {
                var asset = await _assetReps.GetByIdRawAsync(id);
                if (asset != null && asset.IsActive)
                {
                    asset.IsActive = false;
                    asset.ModifiedDate = DateTime.Now;
                    asset.ModifiedBy = _currentUserService.GetDisplayName();
                    await _repository.UpdateAsync(asset);
                    deactivatedCount++;

                    await _activityLogService.LogUpdateAsync(
                        TableNames.Asset,
                        id,
                        $"Asset '{asset.AssetCode}' deactivated via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            return ServiceResult<int>.Success(deactivatedCount,
                $"{deactivatedCount} asset(s) deactivated successfully");
        }, "bulk deactivate assets");
    }

    public async Task<ServiceResult<int>> BulkDeleteAsync(BulkDeleteRequest request)
    {
        if (request.Ids == null || !request.Ids.Any())
        {
            return ServiceResult<int>.BadRequest("No asset IDs provided");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var deletedCount = 0;

            foreach (var id in request.Ids)
            {
                var existing = await _assetReps.GetByIdRawAsync(id);
                if (existing == null)
                {
                    continue;
                }

                await _assetReps.DeleteSoftwareInstallationByAssetIdAsync(id);
                var result = await _repository.DeleteAsync(id);
                if (result > 0)
                {
                    deletedCount++;

                    await _activityLogService.LogDeleteAsync(
                        TableNames.Asset,
                        id,
                        $"Asset '{existing.AssetCode}' - '{existing.AssetName}' deleted via bulk operation",
                        _currentUserService.GetDisplayName());
                }
            }

            var message = deletedCount > 0
                ? $"{deletedCount} asset(s) deleted successfully"
                : "No assets were deleted.";

            return ServiceResult<int>.Success(deletedCount, message);
        }, "bulk delete assets");
    }

    // ============================================================
    // DROPDOWN
    // ============================================================

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
            if (asset == null)
            {
                return ServiceResult<FileContentDto>.NotFound($"Asset with id {assetId} not found");
            }

            var photo = await _assetReps.GetPhotoProfileByAssetIdAsync(assetId);
            if (photo == null)
            {
                return ServiceResult<FileContentDto>.NotFound("Profile photo not found");
            }

            var fileResult = await _fileAttachmentService.GetByIdAsync(photo.FileAttachmentId);
            if (!fileResult.IsSuccess || fileResult.Data == null)
            {
                return ServiceResult<FileContentDto>.NotFound("Profile photo file not found");
            }

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
        if (file == null || file.Length == 0)
        {
            return ServiceResult<string>.BadRequest("No file provided");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!FileSizeConstants.IsAllowedProfilePhotoExtension(extension))
        {
            return ServiceResult<string>.BadRequest(
                $"File type '{extension}' is not allowed. Allowed: {string.Join(", ", FileSizeConstants.AllowedProfilePhotoExtensions)}");
        }

        if (file.Length > FileSizeConstants.MaxProfilePhotoBytes)
        {
            return ServiceResult<string>.BadRequest($"File size exceeds {FileSizeConstants.MaxProfilePhotoMB}MB limit");
        }

        var asset = await _assetReps.GetByIdRawAsync(assetId);
        if (asset == null)
        {
            return ServiceResult<string>.NotFound($"Asset with id {assetId} not found");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            // Soft-delete the previous photo if present.
            var oldPhoto = await _assetReps.GetPhotoProfileByAssetIdAsync(assetId);
            if (oldPhoto != null)
            {
                await _assetReps.DeletePhotoProfileByAssetIdAsync(assetId);
                await _fileAttachmentService.DeleteAsync(oldPhoto.FileAttachmentId);
            }

            // Read the incoming file into a byte array.
            using var stream = file.OpenReadStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var uploadResult = await _fileAttachmentService.UploadBytesAsync(
                referenceTable: "AssetPhotoProfile",
                referenceId: assetId,
                originalFileName: file.FileName,
                bytes: bytes,
                fileCategory: "ProfilePhoto",
                description: $"Profile photo for Asset {assetId}",
                isPrimary: true);

            if (!uploadResult.IsSuccess || uploadResult.Data == null)
            {
                return ServiceResult<string>.Failure(uploadResult.Errors);
            }

            var entity = new AssetPhotoProfileAttachmentEntity
            {
                AssetId = assetId,
                FileAttachmentId = uploadResult.Data.FileAttachmentId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = _currentUserService.GetDisplayName()
            };
            await _assetReps.InsertPhotoProfileAsync(entity);

            await _activityLogService.LogUpdateAsync(
                TableNames.Asset,
                assetId,
                $"Profile photo uploaded",
                _currentUserService.GetDisplayName());

            return ServiceResult<string>.Success(uploadResult.Data.FileUrl ?? string.Empty, "Profile photo uploaded successfully");
        }, "upload asset photo");
    }

    public async Task<ServiceResult> DeletePhotoAsync(int assetId)
    {
        var asset = await _assetReps.GetByIdRawAsync(assetId);
        if (asset == null)
        {
            return ServiceResult.NotFound($"Asset with id {assetId} not found");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var photo = await _assetReps.GetPhotoProfileByAssetIdAsync(assetId);
            if (photo == null)
            {
                return ServiceResult.Success("No profile photo to delete");
            }

            await _assetReps.DeletePhotoProfileByAssetIdAsync(assetId);
            await _fileAttachmentService.DeleteAsync(photo.FileAttachmentId);

            await _activityLogService.LogUpdateAsync(
                TableNames.Asset,
                assetId,
                $"Profile photo deleted",
                _currentUserService.GetDisplayName());

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
            // GetDetailByIdAsync already returns the attachment list (joined with FileAttachment).
            // No redundant repo call needed.
            var detail = await _assetReps.GetDetailByIdAsync(assetId);
            if (detail == null)
            {
                return ServiceResult<IEnumerable<AssetAttachmentViewDto>>.NotFound($"Asset with id {assetId} not found");
            }

            return ServiceResult<IEnumerable<AssetAttachmentViewDto>>.Success(detail.Attachments);
        }, "get asset attachments");
    }

    public async Task<ServiceResult<AssetAttachmentViewDto>> UploadAttachmentAsync(int assetId, AttachmentUploadDto model)
    {
        if (model.File == null || model.File.Length == 0)
        {
            return ServiceResult<AssetAttachmentViewDto>.BadRequest("No file provided");
        }

        var extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
        if (!FileSizeConstants.IsAllowedAssetExtension(extension))
        {
            return ServiceResult<AssetAttachmentViewDto>.BadRequest(
                $"File type '{extension}' is not allowed. Allowed: {string.Join(", ", FileSizeConstants.AllowedAssetExtensions)}");
        }

        if (model.File.Length > FileSizeConstants.MaxAssetAttachmentBytes)
        {
            return ServiceResult<AssetAttachmentViewDto>.BadRequest($"File size exceeds {FileSizeConstants.MaxAssetAttachmentMB}MB limit");
        }

        var asset = await _assetReps.GetByIdRawAsync(assetId);
        if (asset == null)
        {
            return ServiceResult<AssetAttachmentViewDto>.NotFound($"Asset with id {assetId} not found");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            using var stream = model.File.OpenReadStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            var uploadResult = await _fileAttachmentService.UploadBytesAsync(
                referenceTable: "Asset",
                referenceId: assetId,
                originalFileName: model.File.FileName,
                bytes: bytes,
                fileCategory: model.FileCategory ?? "AssetAttachment",
                description: model.Description ?? $"Attachment for Asset {assetId}",
                isPrimary: false);

            if (!uploadResult.IsSuccess || uploadResult.Data == null)
            {
                return ServiceResult<AssetAttachmentViewDto>.Failure(uploadResult.Errors);
            }

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

            await _activityLogService.LogCreateAsync(
                TableNames.Asset,
                assetId,
                $"Attachment uploaded: {model.File.FileName}",
                _currentUserService.GetDisplayName());

            var dto = new AssetAttachmentViewDto
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
            };

            return ServiceResult<AssetAttachmentViewDto>.Success(dto, "Attachment uploaded successfully");
        }, "upload asset attachment");
    }

    public async Task<ServiceResult> DeleteAttachmentAsync(int assetId, int fileAttachmentId)
    {
        var asset = await _assetReps.GetByIdRawAsync(assetId);
        if (asset == null)
        {
            return ServiceResult.NotFound($"Asset with id {assetId} not found");
        }

        var attachments = await _assetReps.GetAttachmentsByAssetIdAsync(assetId);
        var target = attachments.FirstOrDefault(a => a.FileAttachmentId == fileAttachmentId);
        if (target == null)
        {
            return ServiceResult.NotFound($"Attachment {fileAttachmentId} not found for asset {assetId}");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            await _assetReps.DeleteAttachmentAsync(target.AssetAttachmentId);
            await _fileAttachmentService.DeleteAsync(fileAttachmentId);

            await _activityLogService.LogDeleteAsync(
                TableNames.Asset,
                assetId,
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
            if (asset == null)
            {
                return ServiceResult<FileContentDto>.NotFound($"Asset with id {assetId} not found");
            }

            // Ownership check: the attachment row must belong to this asset. Prevents
            // downloading arbitrary FileAttachment ids through a foreign asset route.
            var attachments = await _assetReps.GetAttachmentsByAssetIdAsync(assetId);
            if (!attachments.Any(a => a.FileAttachmentId == fileAttachmentId))
            {
                return ServiceResult<FileContentDto>.NotFound($"Attachment {fileAttachmentId} not found for asset {assetId}");
            }

            // Delegate to the internal FileAttachment service. This is a shared service
            // — no HTTP surface of its own — used by every menu that owns attachments.
            var downloadResult = await _fileAttachmentService.DownloadAsync(fileAttachmentId);
            if (!downloadResult.IsSuccess || downloadResult.Data == null)
            {
                return ServiceResult<FileContentDto>.NotFound($"Attachment file {fileAttachmentId} could not be retrieved");
            }

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
            referenceTable: "AssetPhotoProfile",
            referenceId: assetId,
            originalFileName: fileName,
            bytes: bytes,
            fileCategory: "ProfilePhoto",
            description: $"Profile photo for Asset {assetId}",
            isPrimary: true);

        if (!result.IsSuccess || result.Data == null)
        {
            throw new Exception($"Upload profile photo failed: {string.Join(", ", result.Errors)}");
        }

        var entity = new AssetPhotoProfileAttachmentEntity
        {
            AssetId = assetId,
            FileAttachmentId = result.Data.FileAttachmentId,
            IsActive = true,
            CreatedDate = DateTime.Now,
            CreatedBy = _currentUserService.GetDisplayName()
        };
        await _assetReps.InsertPhotoProfileAsync(entity);

        await _activityLogService.LogUpdateAsync(
            TableNames.Asset,
            assetId,
            $"Profile photo uploaded",
            _currentUserService.GetDisplayName());
    }

    private async Task AddAttachmentFromBase64Async(int assetId, string base64String, string fileName, int order)
    {
        var bytes = Convert.FromBase64String(base64String);

        var fileResult = await _fileAttachmentService.UploadBytesAsync(
            referenceTable: "Asset",
            referenceId: assetId,
            originalFileName: fileName,
            bytes: bytes,
            fileCategory: "AssetAttachment",
            description: $"Attachment for Asset {assetId}",
            isPrimary: false);

        if (!fileResult.IsSuccess || fileResult.Data == null)
        {
            throw new Exception($"Upload attachment failed: {string.Join(", ", fileResult.Errors)}");
        }

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

        await _activityLogService.LogCreateAsync(
            TableNames.Asset,
            assetId,
            $"Attachment added: {fileName}",
            _currentUserService.GetDisplayName());
    }
}