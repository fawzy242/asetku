using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.FileAttachment;

namespace Whitebird.App.Features.FileAttachment;

/// <summary>
/// Service interface for File Attachment business logic
/// </summary>
public interface IFileAttachmentService
{
    /// <summary>
    /// Gets a file attachment by ID
    /// </summary>
    Task<ServiceResult<FileAttachmentDetailViewModel>> GetByIdAsync(int id);

    /// <summary>
    /// Uploads a single file attachment (from IFormFile)
    /// </summary>
    Task<ServiceResult<FileAttachmentResponseDto>> UploadAsync(string referenceTable, int referenceId, FileAttachmentUploadDto model);

    /// <summary>
    /// Uploads a single file attachment from raw bytes.
    /// Used by services that receive base64 payloads (Asset, Employee, AssetTransaction).
    /// </summary>
    Task<ServiceResult<FileAttachmentResponseDto>> UploadBytesAsync(
        string referenceTable,
        int referenceId,
        string originalFileName,
        byte[] bytes,
        string? fileCategory = null,
        string? description = null,
        bool isPrimary = false);

    /// <summary>
    /// Uploads multiple file attachments
    /// </summary>
    Task<ServiceResult<List<FileAttachmentResponseDto>>> UploadMultipleAsync(string referenceTable, int referenceId, FileAttachmentMultipleUploadViewModel model);

    /// <summary>
    /// Updates file attachment metadata (description, isPrimary)
    /// </summary>
    Task<ServiceResult<FileAttachmentResponseDto>> UpdateAsync(int id, FileAttachmentUpdateViewModel model);

    /// <summary>
    /// Deletes a file attachment (soft delete)
    /// </summary>
    Task<ServiceResult> DeleteAsync(int id);

    /// <summary>
    /// Downloads a file attachment
    /// </summary>
    Task<ServiceResult<byte[]>> DownloadAsync(int id);

    /// <summary>
    /// Gets the download URL for a file attachment
    /// </summary>
    Task<ServiceResult<string>> GetFileUrlAsync(int id);

    /// <summary>
    /// Checks if a file attachment is an image (for preview)
    /// </summary>
    Task<ServiceResult<bool>> IsImageFileAsync(int id);
}