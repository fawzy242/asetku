using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.App.Features.FileAttachment;
using Whitebird.App.Features.MasterData;
using Whitebird.Domain.Features.Asset;
using Whitebird.Domain.Features.Common;
using Whitebird.Domain.Features.FileAttachment;
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
    private readonly IMasterDataLookupService _masterDataLookupService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;
    private readonly IStorageService _storageService;
    private readonly IFileAttachmentService _fileAttachmentService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AssetService(
        IGenericRepository<AssetEntity> repository,
        IAssetReps assetReps,
        ICategoryReps categoryReps,
        ISupplierReps supplierReps,
        IMasterDataService masterDataService,
        IMasterDataLookupService masterDataLookupService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        IStorageService storageService,
        IFileAttachmentService fileAttachmentService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AssetService> logger) : base(logger)
    {
        _repository = repository;
        _assetReps = assetReps;
        _categoryReps = categoryReps;
        _supplierReps = supplierReps;
        _masterDataService = masterDataService;
        _masterDataLookupService = masterDataLookupService;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
        _storageService = storageService;
        _fileAttachmentService = fileAttachmentService;
        _httpContextAccessor = httpContextAccessor;
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
                    var os = await _masterDataLookupService.GetPositionNameAsync(software.OperatingSystem.Value);
                    if (os.IsSuccess) asset.SoftwareInstallation.OperatingSystemName = os.Data;
                }
                if (software.MicrosoftOffice.HasValue)
                {
                    var office = await _masterDataLookupService.GetPositionNameAsync(software.MicrosoftOffice.Value);
                    if (office.IsSuccess) asset.SoftwareInstallation.MicrosoftOfficeName = office.Data;
                }
                if (software.VisualStudio.HasValue)
                {
                    var vs = await _masterDataLookupService.GetPositionNameAsync(software.VisualStudio.Value);
                    if (vs.IsSuccess) asset.SoftwareInstallation.VisualStudioName = vs.Data;
                }
                if (software.DBMS.HasValue)
                {
                    var dbms = await _masterDataLookupService.GetPositionNameAsync(software.DBMS.Value);
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

            // CREATE MODE: NO FILE UPLOAD

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

    // ============================================================
    // UPDATE — Handle base64 file upload
    // ============================================================

    public async Task<ServiceResult<AssetDetailView>> UpdateAsync(int id, AssetUpdateFormDto model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _assetReps.GetByIdRawAsync(id);
            if (existing == null)
            {
                return ServiceResult<AssetDetailView>.NotFound($"Asset with id {id} not found");
            }

            var codeExists = await _assetReps.IsAssetCodeExistsAsync(model.AssetCode, id);
            if (codeExists)
            {
                return ServiceResult<AssetDetailView>.Conflict($"Asset code '{model.AssetCode}' already exists");
            }

            var oldCode = existing.AssetCode;
            var oldName = existing.AssetName;

            // Map manual
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

            var result = await _repository.UpdateAsync(existing);
            if (result <= 0)
            {
                return ServiceResult<AssetDetailView>.Failure("Failed to update asset");
            }

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

            // ============================================================
            // HANDLE PHOTO PROFILE (base64)
            // ============================================================
            if (!string.IsNullOrEmpty(model.PhotoFileBase64))
            {
                // Delete existing photo
                var existingPhotos = await _fileAttachmentService.GetByReferenceAsync("AssetPhotoProfile", id);
                if (existingPhotos.IsSuccess && existingPhotos.Data != null && existingPhotos.Data.Any())
                {
                    foreach (var photo in existingPhotos.Data)
                    {
                        await _fileAttachmentService.DeleteAsync(photo.FileAttachmentId);
                    }
                }

                // Convert base64 to file and upload
                await AddPhotoProfileFromBase64Async(id, model.PhotoFileBase64, model.PhotoFileName ?? "photo.jpg");
            }

            // ============================================================
            // HANDLE ATTACHMENTS (base64)
            // ============================================================
            if (model.ReplaceAttachments || (model.AttachmentFilesBase64 != null && model.AttachmentFilesBase64.Any()))
            {
                var existingAttachments = await _assetReps.GetAttachmentsByAssetIdAsync(id);
                var keepIds = model.AttachmentIdsToKeep ?? new List<int>();

                foreach (var att in existingAttachments)
                {
                    if (!keepIds.Contains(att.FileAttachmentId))
                    {
                        await _fileAttachmentService.DeleteAsync(att.FileAttachmentId);
                        await _assetReps.DeleteAttachmentAsync(att.AssetAttachmentId);
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
                $"Asset updated: Code '{oldCode}' -> '{model.AssetCode}', Name '{oldName}' -> '{model.AssetName}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetDetailView>.Success(updated!, "Asset updated successfully");
        }, "update asset");
    }

    // ============================================================
    // DELETE & SOFT DELETE
    // ============================================================

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
    // PHOTO PROFILE
    // ============================================================

    public async Task<ServiceResult> AddPhotoProfileAsync(int assetId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return ServiceResult.BadRequest("No file provided");
        }

        var asset = await _assetReps.GetByIdRawAsync(assetId);
        if (asset == null)
        {
            return ServiceResult.NotFound($"Asset with id {assetId} not found");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!FileSizeConstants.IsAllowedProfilePhotoExtension(extension))
        {
            return ServiceResult.BadRequest($"File type '{extension}' is not allowed. Allowed: {string.Join(", ", FileSizeConstants.AllowedProfilePhotoExtensions)}");
        }

        if (file.Length > FileSizeConstants.MaxProfilePhotoBytes)
        {
            return ServiceResult.BadRequest($"File size exceeds {FileSizeConstants.MaxProfilePhotoMB}MB limit");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var subDirectory = Path.Combine("Asset", assetId.ToString(), "Profile");
            var savedPath = await _storageService.SaveFileAsync(file, subDirectory);
            var fileHash = _storageService.ComputeFileHash(file);

            var uploadModel = new FileAttachmentUploadDto
            {
                File = file,
                FileCategory = "ProfilePhoto",
                Description = $"Profile photo for Asset {asset.AssetCode}",
                IsPrimary = true
            };

            var result = await _fileAttachmentService.UploadAsync("AssetPhotoProfile", assetId, uploadModel);
            if (!result.IsSuccess)
            {
                return ServiceResult.Failure("Failed to upload photo profile");
            }

            await _activityLogService.LogUpdateAsync(
                TableNames.Asset,
                assetId,
                $"Profile photo uploaded for Asset '{asset.AssetCode}'",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Profile photo uploaded successfully");
        }, "add asset photo profile");
    }

    private async Task AddPhotoProfileFromBase64Async(int assetId, string base64String, string fileName)
    {
        var bytes = Convert.FromBase64String(base64String);
        using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = GetContentType(fileName)
        };
        await AddPhotoProfileAsync(assetId, file);
    }

    private async Task AddAttachmentFromBase64Async(int assetId, string base64String, string fileName, int order)
    {
        var bytes = Convert.FromBase64String(base64String);
        using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = GetContentType(fileName)
        };
        await AddAttachmentAsync(assetId, file, $"Attachment: {fileName}", order);
    }

    private string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    public async Task<ServiceResult> DeletePhotoProfileAsync(int assetId)
    {
        var asset = await _assetReps.GetByIdRawAsync(assetId);
        if (asset == null)
        {
            return ServiceResult.NotFound($"Asset with id {assetId} not found");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var existingPhotos = await _fileAttachmentService.GetByReferenceAsync("AssetPhotoProfile", assetId);
            if (!existingPhotos.IsSuccess || existingPhotos.Data == null || !existingPhotos.Data.Any())
            {
                return ServiceResult.Success("No profile photo to delete");
            }

            foreach (var photo in existingPhotos.Data)
            {
                await _fileAttachmentService.DeleteAsync(photo.FileAttachmentId);
            }

            await _activityLogService.LogUpdateAsync(
                TableNames.Asset,
                assetId,
                $"Profile photo deleted for Asset '{asset.AssetCode}'",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Profile photo deleted successfully");
        }, "delete asset photo profile");
    }

    public async Task<ServiceResult<byte[]>> GetPhotoProfileAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetByIdRawAsync(assetId);
            if (asset == null)
            {
                return ServiceResult<byte[]>.NotFound($"Asset with id {assetId} not found");
            }

            var existingPhotos = await _fileAttachmentService.GetByReferenceAsync("AssetPhotoProfile", assetId);
            if (!existingPhotos.IsSuccess || existingPhotos.Data == null || !existingPhotos.Data.Any())
            {
                return ServiceResult<byte[]>.NotFound("Profile photo not found");
            }

            var primaryPhoto = existingPhotos.Data.FirstOrDefault(p => p.IsPrimary) ?? existingPhotos.Data.First();

            var fileResult = await _fileAttachmentService.GetByIdAsync(primaryPhoto.FileAttachmentId);
            if (!fileResult.IsSuccess || fileResult.Data == null)
            {
                return ServiceResult<byte[]>.NotFound("Profile photo file not found");
            }

            var fileBytes = await _storageService.ReadFileAsync(fileResult.Data.FilePath);
            return ServiceResult<byte[]>.Success(fileBytes);
        }, "get asset photo profile");
    }

    // ============================================================
    // ASSET ATTACHMENT
    // ============================================================

    public async Task<ServiceResult<AssetAttachmentEntity>> AddAttachmentAsync(int assetId, IFormFile file, string? description = null, int order = 0)
    {
        if (file == null || file.Length == 0)
        {
            return ServiceResult<AssetAttachmentEntity>.BadRequest("No file provided");
        }

        var asset = await _assetReps.GetByIdRawAsync(assetId);
        if (asset == null)
        {
            return ServiceResult<AssetAttachmentEntity>.NotFound($"Asset with id {assetId} not found");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!FileSizeConstants.IsAllowedAssetExtension(extension))
        {
            return ServiceResult<AssetAttachmentEntity>.BadRequest($"File type '{extension}' is not allowed. Allowed: {string.Join(", ", FileSizeConstants.AllowedAssetExtensions)}");
        }

        if (file.Length > FileSizeConstants.MaxAssetAttachmentBytes)
        {
            return ServiceResult<AssetAttachmentEntity>.BadRequest($"File size exceeds {FileSizeConstants.MaxAssetAttachmentMB}MB limit");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var subDirectory = Path.Combine("Asset", assetId.ToString(), "Attachments");
            var savedPath = await _storageService.SaveFileAsync(file, subDirectory);
            var fileHash = _storageService.ComputeFileHash(file);

            var uploadModel = new FileAttachmentUploadDto
            {
                File = file,
                FileCategory = "AssetAttachment",
                Description = description ?? $"Attachment for Asset {asset.AssetCode}",
                IsPrimary = false
            };

            var fileResult = await _fileAttachmentService.UploadAsync("Asset", assetId, uploadModel);
            if (!fileResult.IsSuccess || fileResult.Data == null)
            {
                return ServiceResult<AssetAttachmentEntity>.Failure("Failed to upload file");
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

            var attachmentId = await _assetReps.InsertAttachmentAsync(attachmentEntity);
            var created = await _assetReps.GetAttachmentByIdAsync(attachmentId);

            await _activityLogService.LogCreateAsync(
                TableNames.Asset,
                assetId,
                $"Attachment added for Asset '{asset.AssetCode}': {file.FileName}",
                _currentUserService.GetDisplayName());

            return ServiceResult<AssetAttachmentEntity>.Success(created!, "Attachment added successfully");
        }, "add asset attachment");
    }

    public async Task<ServiceResult<IEnumerable<AssetAttachmentEntity>>> GetAttachmentsAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetByIdRawAsync(assetId);
            if (asset == null)
            {
                return ServiceResult<IEnumerable<AssetAttachmentEntity>>.NotFound($"Asset with id {assetId} not found");
            }

            var attachments = await _assetReps.GetAttachmentsByAssetIdAsync(assetId);
            return ServiceResult<IEnumerable<AssetAttachmentEntity>>.Success(attachments);
        }, "get asset attachments");
    }

    public async Task<ServiceResult> DeleteAttachmentAsync(int assetId, int attachmentId)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var asset = await _assetReps.GetByIdRawAsync(assetId);
            if (asset == null)
            {
                return ServiceResult.NotFound($"Asset with id {assetId} not found");
            }

            var attachment = await _assetReps.GetAttachmentByIdAsync(attachmentId);
            if (attachment == null || attachment.AssetId != assetId)
            {
                return ServiceResult.NotFound($"Attachment with id {attachmentId} not found for asset {assetId}");
            }

            var fileResult = await _fileAttachmentService.DeleteAsync(attachment.FileAttachmentId);
            if (!fileResult.IsSuccess)
            {
                return ServiceResult.Failure("Failed to delete file attachment");
            }

            await _assetReps.DeleteAttachmentAsync(attachmentId);

            await _activityLogService.LogDeleteAsync(
                TableNames.Asset,
                assetId,
                $"Attachment {attachmentId} deleted for Asset '{asset.AssetCode}'",
                _currentUserService.GetDisplayName());

            return ServiceResult.Success("Attachment deleted successfully");
        }, "delete asset attachment");
    }
}