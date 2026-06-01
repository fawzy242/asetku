using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.FileAttachment;
using Whitebird.Domain.Features.Common;
using Whitebird.Infra.Features.FileAttachment;

namespace Whitebird.App.Features.FileAttachment;

/// <summary>
/// Service implementation for File Attachment business logic
/// </summary>
public class FileAttachmentService : BaseService, IFileAttachmentService
{
    private readonly IFileAttachmentReps _fileAttachmentReps;
    private readonly IStorageService _storageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;
    private readonly IHttpContextAccessor _httpContextAccessor;

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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<ServiceResult<IEnumerable<FileAttachmentListViewModel>>> GetByReferenceAsync(string referenceTable, int referenceId)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var attachments = await _fileAttachmentReps.GetByReferenceAsync(referenceTable, referenceId);
            return ServiceResult<IEnumerable<FileAttachmentListViewModel>>.Success(attachments.Adapt<IEnumerable<FileAttachmentListViewModel>>());
        }, "get file attachments by reference");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<FileAttachmentResponseDto>> UploadAsync(string referenceTable, int referenceId, FileAttachmentUploadViewModel model)
    {
        if (model.File == null || model.File.Length == 0)
        {
            return ServiceResult<FileAttachmentResponseDto>.BadRequest("No file provided");
        }

        var extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
        if (!FileSizeConstants.IsAllowedAssetExtension(extension))
        {
            return ServiceResult<FileAttachmentResponseDto>.BadRequest($"File type '{extension}' is not allowed. Allowed: {string.Join(", ", FileSizeConstants.AllowedAssetExtensions)}");
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

            var existingCount = await _fileAttachmentReps.GetCountByReferenceAsync(referenceTable, referenceId);
            var shouldBePrimary = model.IsPrimary || existingCount == 0;

            if (shouldBePrimary)
            {
                await _fileAttachmentReps.SetPrimaryAsync(0, referenceTable, referenceId);
            }

            var entity = new FileAttachmentEntity
            {
                ReferenceTable = referenceTable,
                ReferenceId = referenceId,
                FileCategory = model.FileCategory,
                FileName = Path.GetFileName(savedPath),
                OriginalFileName = model.File.FileName,
                FileExtension = extension,
                FileMimeType = _storageService.GetMimeType(model.File.FileName),
                FilePath = savedPath,
                FileSize = model.File.Length,
                FileHash = fileHash,
                Description = model.Description,
                VersionNumber = 1,
                IsPrimary = shouldBePrimary,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = _currentUserService.GetDisplayName()
            };

            var id = await _fileAttachmentReps.InsertAsync(entity);

            var request = _httpContextAccessor.HttpContext?.Request;
            var fileUrl = request != null ? $"{request.Scheme}://{request.Host}/api/FileAttachment/download/{id}" : null;
            var previewUrl = _storageService.IsImageFile(model.File.FileName) && request != null
                ? $"{request.Scheme}://{request.Host}/api/FileAttachment/preview/{id}"
                : null;

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
                $"File uploaded: '{model.File.FileName}'",
                _currentUserService.GetDisplayName());

            return ServiceResult<FileAttachmentResponseDto>.Success(response, "File uploaded successfully");
        }, "upload file", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(referenceTable, referenceId, "Upload File", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
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
            var uploadModel = new FileAttachmentUploadViewModel
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

    /// <inheritdoc />
    public async Task<ServiceResult<FileAttachmentResponseDto>> UpdateAsync(int id, FileAttachmentUpdateViewModel model)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _fileAttachmentReps.GetByIdAsync(id);
            if (existing == null)
            {
                return ServiceResult<FileAttachmentResponseDto>.NotFound($"Attachment with id {id} not found");
            }

            var oldDescription = existing.Description;
            var oldIsPrimary = existing.IsPrimary;

            existing.Description = model.Description;
            existing.IsPrimary = model.IsPrimary;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = _currentUserService.GetDisplayName();

            if (model.IsPrimary && !oldIsPrimary)
            {
                await _fileAttachmentReps.SetPrimaryAsync(id, existing.ReferenceTable, existing.ReferenceId);
            }

            var result = await _fileAttachmentReps.UpdateAsync(existing);
            if (result <= 0)
            {
                return ServiceResult<FileAttachmentResponseDto>.Failure("Failed to update attachment metadata");
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            var response = new FileAttachmentResponseDto
            {
                FileAttachmentId = existing.FileAttachmentId,
                FileName = existing.FileName,
                OriginalFileName = existing.OriginalFileName,
                FileUrl = request != null ? $"{request.Scheme}://{request.Host}/api/FileAttachment/download/{id}" : null,
                PreviewUrl = _storageService.IsImageFile(existing.OriginalFileName) && request != null
                    ? $"{request.Scheme}://{request.Host}/api/FileAttachment/preview/{id}"
                    : null,
                FileSize = existing.FileSize,
                FileCategory = existing.FileCategory,
                IsPrimary = existing.IsPrimary
            };

            await _activityLogService.LogUpdateAsync(
                existing.ReferenceTable,
                existing.ReferenceId,
                $"File attachment updated: '{existing.OriginalFileName}' - IsPrimary: {oldIsPrimary} -> {model.IsPrimary}",
                _currentUserService.GetDisplayName());

            return ServiceResult<FileAttachmentResponseDto>.Success(response, "Attachment updated successfully");
        }, "update file attachment", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.FileAttachment, id, "Update Attachment", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var existing = await _fileAttachmentReps.GetByIdAsync(id);
            if (existing == null)
            {
                return ServiceResult.NotFound($"Attachment with id {id} not found");
            }

            await _storageService.DeleteFileAsync(existing.FilePath);
            var result = await _fileAttachmentReps.DeleteAsync(id);

            if (result > 0)
            {
                await _activityLogService.LogDeleteAsync(
                    existing.ReferenceTable,
                    existing.ReferenceId,
                    $"File attachment deleted: '{existing.OriginalFileName}'",
                    _currentUserService.GetDisplayName());
            }

            return result <= 0
                ? ServiceResult.Failure("Failed to delete attachment")
                : ServiceResult.Success("Attachment deleted successfully");
        }, "delete file attachment", async (ex) =>
        {
            await _activityLogService.LogErrorAsync(TableNames.FileAttachment, id, "Delete Attachment", ex, _currentUserService.GetDisplayName());
        });
    }

    /// <inheritdoc />
    public async Task<ServiceResult<byte[]>> DownloadAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var attachment = await _fileAttachmentReps.GetByIdAsync(id);
            if (attachment == null)
            {
                return ServiceResult<byte[]>.NotFound($"Attachment with id {id} not found");
            }

            var fileBytes = await _storageService.ReadFileAsync(attachment.FilePath);
            return ServiceResult<byte[]>.Success(fileBytes);
        }, "download file");
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string>> GetFileUrlAsync(int id)
    {
        return await ExecuteSafelyAsync(async () =>
        {
            var attachment = await _fileAttachmentReps.GetByIdAsync(id);
            if (attachment == null)
            {
                return ServiceResult<string>.NotFound($"Attachment with id {id} not found");
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            var url = request != null ? $"{request.Scheme}://{request.Host}/api/FileAttachment/download/{id}" : null;
            return ServiceResult<string>.Success(url ?? string.Empty);
        }, "get file url");
    }

    /// <inheritdoc />
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
}