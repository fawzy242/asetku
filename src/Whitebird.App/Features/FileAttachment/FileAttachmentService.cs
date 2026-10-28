using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.FileAttachment;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.FileAttachment;

namespace Whitebird.App.Features.FileAttachment;

/// <summary>
/// Internal service for file attachment storage and metadata.
///
/// This is NOT an API module — each owning menu (Asset, Employee,
/// AssetTransaction, User) exposes its own upload / download / delete
/// endpoints and delegates to this service internally. Consequently:
///
///   * DownloadAsync returns bytes + content type + file name so callers can
///     stream the file through their own controller endpoint.
///   * FileUrl in FileAttachmentResponseDto always resolves to the owning
///     menu's download endpoint (see BuildFileUrl).
///
/// Storage-layer extension validation (H6 fix):
///   This service is the shared storage primitive for BOTH profile photos and
///   menu attachments. Callers validate the correct profile-specific rules
///   (FileUploadValidator.ValidateProfilePhoto / ValidateAssetAttachment) at
///   the boundary before reaching here. The service performs a defence-in-depth
///   check that accepts any extension from either allow-list (asset-attachment
///   OR profile-photo). The previous version hardcoded the asset-attachment list
///   and wrongly rejected .webp profile photos.
/// </summary>
public class FileAttachmentService : BaseService, IFileAttachmentService
{
    private readonly IFileAttachmentReps _fileAttachmentReps;
    private readonly IStorageService _storageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Union of the two allow-lists, computed once per process. Kept as a static
    // readonly field so the rejection message construction is allocation-free on
    // the success path.
    private static readonly string[] _allowedUploadExtensions = BuildAllowedUploadExtensions();
    private static readonly string _allowedUploadExtensionsDisplay = string.Join(", ", _allowedUploadExtensions);

    public FileAttachmentService(
        IFileAttachmentReps fileAttachmentReps,
        IStorageService storageService,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<FileAttachmentService> logger) : base(logger)
    {
        _fileAttachmentReps = fileAttachmentReps;
        _storageService = storageService;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
        _httpContextAccessor = httpContextAccessor;
    }

    // ============================================================
    // METADATA
    // ============================================================

    public async Task<ServiceResult<FileAttachmentDetailViewModel>> GetByIdAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var attachment = await _fileAttachmentReps.GetByIdAsync(id);
            if (attachment == null)
            {
                return ServiceResult<FileAttachmentDetailViewModel>.NotFound($"Attachment with id {id} not found");
            }

            return ServiceResult<FileAttachmentDetailViewModel>.Success(attachment.Adapt<FileAttachmentDetailViewModel>());
        }, "get file attachment by id");
    }

    // ============================================================
    // UPLOAD
    // ============================================================

    public async Task<ServiceResult<FileAttachmentResponseDto>> UploadAsync(string referenceTable, int referenceId, FileAttachmentUploadDto model)
    {
        if (model.File == null || model.File.Length == 0)
        {
            return ServiceResult<FileAttachmentResponseDto>.BadRequest("No file provided");
        }

        var extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
        if (!IsAllowedUploadExtension(extension))
        {
            return ServiceResult<FileAttachmentResponseDto>.BadRequest(
                $"File type '{extension}' is not allowed. Allowed: {_allowedUploadExtensionsDisplay}");
        }

        if (model.File.Length > FileSizeConstants.MaxAssetAttachmentBytes)
        {
            return ServiceResult<FileAttachmentResponseDto>.BadRequest($"File size exceeds {FileSizeConstants.MaxAssetAttachmentMB}MB limit");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var subDirectory = Path.Combine(referenceTable, referenceId.ToString());
            var savedPath = await _storageService.SaveFileAsync(model.File, subDirectory);
            var fileHash = _storageService.ComputeFileHash(model.File);

            return await PersistAsync(
                referenceTable,
                referenceId,
                originalFileName: model.File.FileName,
                extension: extension,
                savedPath: savedPath,
                fileSize: model.File.Length,
                fileHash: fileHash,
                fileCategory: model.FileCategory,
                description: model.Description,
                isPrimary: model.IsPrimary);
        }, "upload file");
    }

    public async Task<ServiceResult<FileAttachmentResponseDto>> UploadBytesAsync(
        string referenceTable,
        int referenceId,
        string originalFileName,
        byte[] bytes,
        string? fileCategory = null,
        string? description = null,
        bool isPrimary = false)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return ServiceResult<FileAttachmentResponseDto>.BadRequest("No file content provided");
        }

        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            return ServiceResult<FileAttachmentResponseDto>.BadRequest("Original file name is required");
        }

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!IsAllowedUploadExtension(extension))
        {
            return ServiceResult<FileAttachmentResponseDto>.BadRequest(
                $"File type '{extension}' is not allowed. Allowed: {_allowedUploadExtensionsDisplay}");
        }

        if (bytes.Length > FileSizeConstants.MaxAssetAttachmentBytes)
        {
            return ServiceResult<FileAttachmentResponseDto>.BadRequest(
                $"File size exceeds {FileSizeConstants.MaxAssetAttachmentMB}MB limit");
        }

        return await ExecuteWithTransactionAsync(async () =>
        {
            var subDirectory = Path.Combine(referenceTable, referenceId.ToString());
            var savedPath = await _storageService.SaveBytesAsync(bytes, originalFileName, subDirectory);
            var fileHash = _storageService.ComputeHash(bytes);

            return await PersistAsync(
                referenceTable,
                referenceId,
                originalFileName: originalFileName,
                extension: extension,
                savedPath: savedPath,
                fileSize: bytes.Length,
                fileHash: fileHash,
                fileCategory: fileCategory,
                description: description,
                isPrimary: isPrimary);
        }, "upload file bytes");
    }

    public async Task<ServiceResult<List<FileAttachmentResponseDto>>> UploadMultipleAsync(string referenceTable, int referenceId, FileAttachmentMultipleUploadViewModel model)
    {
        if (model.Files == null || model.Files.Count == 0)
        {
            return ServiceResult<List<FileAttachmentResponseDto>>.BadRequest("No files provided");
        }

        var results = new List<FileAttachmentResponseDto>();
        var errors = new List<string>();

        foreach (var file in model.Files)
        {
            var uploadModel = new FileAttachmentUploadDto
            {
                File = file,
                FileCategory = model.FileCategory,
                Description = model.Description,
                IsPrimary = false
            };

            var result = await UploadAsync(referenceTable, referenceId, uploadModel);
            if (result.IsSuccess && result.Data != null)
            {
                results.Add(result.Data);
            }
            else
            {
                errors.Add($"File '{file.FileName}': {string.Join(", ", result.Errors)}");
            }
        }

        if (results.Count == 0)
        {
            return ServiceResult<List<FileAttachmentResponseDto>>.Failure(errors, "No files were uploaded successfully");
        }

        var message = $"Successfully uploaded {results.Count} of {model.Files.Count} files";
        return ServiceResult<List<FileAttachmentResponseDto>>.Success(results, message);
    }

    // ============================================================
    // METADATA UPDATE / DELETE
    // ============================================================

    public async Task<ServiceResult<FileAttachmentResponseDto>> UpdateAsync(int id, FileAttachmentUpdateViewModel model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _fileAttachmentReps.GetByIdAsync(id);
            if (existing == null)
            {
                return ServiceResult<FileAttachmentResponseDto>.NotFound($"Attachment with id {id} not found");
            }

            var oldIsPrimary = existing.IsPrimary;

            existing.Description = model.Description;
            existing.IsPrimary = model.IsPrimary;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            var result = await _fileAttachmentReps.UpdateAsync(existing);
            if (result <= 0)
            {
                return ServiceResult<FileAttachmentResponseDto>.Failure("Failed to update attachment metadata");
            }

            // No reference context here (no referenceTable / referenceId), so we cannot
            // build a valid per-menu download URL. FileUrl / PreviewUrl are left null;
            // callers should resolve URLs via the owning menu's endpoint.
            var response = new FileAttachmentResponseDto
            {
                FileAttachmentId = existing.FileAttachmentId,
                FileName = existing.FileName,
                OriginalFileName = existing.OriginalFileName,
                FileUrl = null,
                PreviewUrl = null,
                FileSize = existing.FileSize,
                FileCategory = existing.FileCategory,
                IsPrimary = existing.IsPrimary
            };

            await _activityLogService.LogUpdateAsync(
                "FileAttachment",
                id,
                $"File attachment updated: '{existing.OriginalFileName}' - IsPrimary: {oldIsPrimary} -> {model.IsPrimary}",
                _currentUserService.GetDisplayName());

            return ServiceResult<FileAttachmentResponseDto>.Success(response, "Attachment updated successfully");
        }, "update file attachment");
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _fileAttachmentReps.GetByIdAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Attachment with id {id} not found");
            }

            var result = await _fileAttachmentReps.DeleteAsync(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(
                    "FileAttachment",
                    id,
                    $"File attachment deleted: '{existing.OriginalFileName}'",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to delete attachment")
                : ServiceResult.Success("Attachment deleted successfully");
        }, "delete file attachment");
    }

    // ============================================================
    // DOWNLOAD
    // ============================================================

    public async Task<ServiceResult<FileContentDto>> DownloadAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var attachment = await _fileAttachmentReps.GetByIdAsync(id);
            if (attachment == null)
            {
                return ServiceResult<FileContentDto>.NotFound($"Attachment with id {id} not found");
            }

            var fileBytes = await _storageService.ReadFileAsync(attachment.FilePath);

            var contentType = !string.IsNullOrEmpty(attachment.FileMimeType)
                ? attachment.FileMimeType
                : _storageService.GetMimeType(attachment.OriginalFileName);

            return ServiceResult<FileContentDto>.Success(new FileContentDto
            {
                Bytes = fileBytes,
                ContentType = contentType,
                FileName = attachment.OriginalFileName
            });
        }, "download file");
    }

    public Task<ServiceResult<string>> GetFileUrlAsync(int id)
    {
        // URLs are context-dependent: the correct download route is owned by the
        // menu the file belongs to. This context-blind overload cannot produce
        // a valid URL under the current architecture.
        return Task.FromResult(ServiceResult<string>.NotFound(
            "File URLs are resolved through the owning menu's download endpoint. " +
            "Use the FileUrl returned by the upload response, or construct the " +
            "owning menu's endpoint (e.g. /api/Asset/{assetId}/attachments/{fileAttachmentId}/download)."));
    }

    public async Task<ServiceResult<bool>> IsImageFileAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var attachment = await _fileAttachmentReps.GetByIdAsync(id);
            if (attachment == null)
            {
                return ServiceResult<bool>.NotFound($"Attachment with id {id} not found");
            }

            var isImage = _storageService.IsImageFile(attachment.OriginalFileName);
            return ServiceResult<bool>.Success(isImage);
        }, "check if file is image");
    }

    // ============================================================
    // EXTENSION HELPERS (H6 fix)
    // ============================================================

    /// <summary>
    /// Union of the asset-attachment and profile-photo allow-lists. The storage
    /// layer is a shared primitive; callers validate the flow-specific rule
    /// upstream (FileUploadValidator), and this service accepts any extension
    /// that either list permits. Previously this hardcoded the asset-attachment
    /// list and wrongly rejected .webp profile photos.
    /// </summary>
    private static string[] BuildAllowedUploadExtensions()
    {
        var union = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var ext in FileSizeConstants.AllowedAssetExtensions) union.Add(ext);
        foreach (var ext in FileSizeConstants.AllowedProfilePhotoExtensions) union.Add(ext);
        return union.ToArray();
    }

    private static bool IsAllowedUploadExtension(string extension)
    {
        if (string.IsNullOrEmpty(extension)) return false;
        return Array.IndexOf(_allowedUploadExtensions, extension) >= 0;
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================

    private async Task<ServiceResult<FileAttachmentResponseDto>> PersistAsync(
        string referenceTable,
        int referenceId,
        string originalFileName,
        string extension,
        string savedPath,
        long fileSize,
        string fileHash,
        string? fileCategory,
        string? description,
        bool isPrimary)
    {
        var entity = new FileAttachmentEntity
        {
            FileCategory = fileCategory,
            FileName = Path.GetFileName(savedPath),
            OriginalFileName = originalFileName,
            FileExtension = extension,
            FileMimeType = _storageService.GetMimeType(originalFileName),
            FilePath = savedPath,
            FileSize = fileSize,
            FileHash = fileHash,
            Description = description,
            VersionNumber = 1,
            IsPrimary = isPrimary,
            IsActive = true,
            CreatedDate = DateTime.Now,
            CreatedBy = _currentUserService.GetDisplayName()
        };

        var id = await _fileAttachmentReps.InsertAsync(entity);

        var fileUrl = BuildFileUrl(referenceTable, referenceId, id);
        var previewUrl = fileUrl != null && _storageService.IsImageFile(originalFileName) ? fileUrl : null;

        var response = new FileAttachmentResponseDto
        {
            FileAttachmentId = id,
            FileName = entity.FileName,
            OriginalFileName = entity.OriginalFileName,
            FileUrl = fileUrl,
            PreviewUrl = previewUrl,
            FileSize = entity.FileSize,
            FileCategory = entity.FileCategory,
            IsPrimary = entity.IsPrimary
        };

        await _activityLogService.LogCreateAsync(
            referenceTable,
            referenceId,
            $"File uploaded: '{originalFileName}'",
            _currentUserService.GetDisplayName());

        return ServiceResult<FileAttachmentResponseDto>.Success(response, "File uploaded successfully");
    }

    private string? BuildFileUrl(string referenceTable, int referenceId, int fileAttachmentId)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null) return null;
        var baseUrl = $"{request.Scheme}://{request.Host}";

        return referenceTable switch
        {
            "Asset" => $"{baseUrl}/api/Asset/{referenceId}/attachments/{fileAttachmentId}/download",
            "AssetPhotoProfile" => $"{baseUrl}/api/Asset/{referenceId}/photo",
            "Employee" => $"{baseUrl}/api/Employee/{referenceId}/attachments/{fileAttachmentId}/download",
            "EmployeePhotoProfile" => $"{baseUrl}/api/Employee/{referenceId}/photo",
            "AssetTransaction" => $"{baseUrl}/api/AssetTransaction/{referenceId}/attachments/{fileAttachmentId}/download",
            "AssetTransactionApprove" => $"{baseUrl}/api/AssetTransaction/{referenceId}/approve-attachments/{fileAttachmentId}/download",
            _ => null
        };
    }
}