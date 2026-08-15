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
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
        _storageService = storageService;
        _fileAttachmentService = fileAttachmentService;
        _httpContextAccessor = httpContextAccessor;
    }

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

            // Update basic fields
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

            // Software Installation
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
            // PHOTO PROFILE
            // ============================================================
            if (!string.IsNullOrEmpty(model.PhotoFileBase64))
            {
                var oldPhoto = await _assetReps.GetPhotoProfileByAssetIdAsync(id);
                if (oldPhoto != null)
                {
                    await _fileAttachmentService.DeleteAsync(oldPhoto.FileAttachmentId);
                    await _assetReps.DeletePhotoProfileByAssetIdAsync(id);
                }

                await AddPhotoProfileFromBase64Async(id, model.PhotoFileBase64, model.PhotoFileName ?? "photo.jpg");
            }

            // ============================================================
            // ATTACHMENTS
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

    public async Task<ServiceResult<IEnumerable<AssetDropdownView>>> GetDropdownListByTransactionTypeAsync(int transactionType)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var assets = await _assetReps.GetDropdownListByTransactionTypeAsync(transactionType);
            return ServiceResult<IEnumerable<AssetDropdownView>>.Success(assets);
        }, "get asset dropdown by transaction type");
    }

    // ============================================================
    // PRIVATE METHODS — BASE64 UPLOAD
    // ============================================================

    private async Task AddPhotoProfileFromBase64Async(int assetId, string base64String, string fileName)
    {
        try
        {
            var bytes = Convert.FromBase64String(base64String);
            using var stream = new MemoryStream(bytes);
            var file = new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = GetContentType(fileName)
            };

            var uploadModel = new FileAttachmentUploadDto
            {
                File = file,
                FileCategory = "ProfilePhoto",
                Description = $"Profile photo for Asset {assetId}",
                IsPrimary = true
            };

            var result = await _fileAttachmentService.UploadAsync("AssetPhotoProfile", assetId, uploadModel);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading photo for Asset {AssetId}", assetId);
            throw;
        }
    }

    private async Task AddAttachmentFromBase64Async(int assetId, string base64String, string fileName, int order)
    {
        try
        {
            var bytes = Convert.FromBase64String(base64String);
            using var stream = new MemoryStream(bytes);
            var file = new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = GetContentType(fileName)
            };

            var uploadModel = new FileAttachmentUploadDto
            {
                File = file,
                FileCategory = "AssetAttachment",
                Description = $"Attachment for Asset {assetId}",
                IsPrimary = false
            };

            var fileResult = await _fileAttachmentService.UploadAsync("Asset", assetId, uploadModel);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding attachment for Asset {AssetId}", assetId);
            throw;
        }
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

    // ============================================================
    // NEW: PHOTO & ATTACHMENTS (for AssetTracking and standalone use)
    // ============================================================

    public async Task<ServiceResult<byte[]>> GetPhotoAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetByIdRawAsync(assetId);
            if (asset == null)
            {
                return ServiceResult<byte[]>.NotFound($"Asset with id {assetId} not found");
            }

            var photo = await _assetReps.GetPhotoProfileByAssetIdAsync(assetId);
            if (photo == null)
            {
                return ServiceResult<byte[]>.NotFound("Profile photo not found");
            }

            var fileResult = await _fileAttachmentService.GetByIdAsync(photo.FileAttachmentId);
            if (!fileResult.IsSuccess || fileResult.Data == null)
            {
                return ServiceResult<byte[]>.NotFound("Profile photo file not found");
            }

            var fileBytes = await _storageService.ReadFileAsync(fileResult.Data.FilePath);
            return ServiceResult<byte[]>.Success(fileBytes);
        }, "get asset photo");
    }

    public async Task<ServiceResult<IEnumerable<AssetAttachmentViewDto>>> GetAttachmentsAsync(int assetId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var asset = await _assetReps.GetByIdRawAsync(assetId);
            if (asset == null)
            {
                return ServiceResult<IEnumerable<AssetAttachmentViewDto>>.NotFound($"Asset with id {assetId} not found");
            }

            // Get attachments from repository (joins with FileAttachment)
            var attachments = await _assetReps.GetAttachmentsByAssetIdAsync(assetId);

            // Map to DTO using the GetDetailByIdAsync approach
            var result = new List<AssetAttachmentViewDto>();

            // Use the same query pattern as GetDetailByIdAsync for attachments
            // We need to get FileAttachment data via the repository
            var detail = await _assetReps.GetDetailByIdAsync(assetId);
            if (detail != null)
            {
                result = detail.Attachments.ToList();
            }

            return ServiceResult<IEnumerable<AssetAttachmentViewDto>>.Success(result);
        }, "get asset attachments");
    }
}