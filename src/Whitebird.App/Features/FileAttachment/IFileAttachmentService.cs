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
    /// <param name="id">File attachment ID</param>
    /// <returns>File attachment detail view model or not found result</returns>
    Task<ServiceResult<FileAttachmentDetailViewModel>> GetByIdAsync(int id);

    /// <summary>
    /// Gets all file attachments for a specific reference (table + id)
    /// </summary>
    /// <param name="referenceTable">Reference table name</param>
    /// <param name="referenceId">Reference record ID</param>
    /// <returns>Collection of file attachment list view models</returns>
    Task<ServiceResult<IEnumerable<FileAttachmentListViewModel>>> GetByReferenceAsync(string referenceTable, int referenceId);

    /// <summary>
    /// Uploads a single file attachment
    /// </summary>
    /// <param name="referenceTable">Reference table name</param>
    /// <param name="referenceId">Reference record ID</param>
    /// <param name="model">Upload data including file and metadata</param>
    /// <returns>File attachment response DTO with download URL</returns>
    Task<ServiceResult<FileAttachmentResponseDto>> UploadAsync(string referenceTable, int referenceId, FileAttachmentUploadViewModel model);

    /// <summary>
    /// Uploads multiple file attachments
    /// </summary>
    /// <param name="referenceTable">Reference table name</param>
    /// <param name="referenceId">Reference record ID</param>
    /// <param name="model">Multiple upload data</param>
    /// <returns>List of file attachment response DTOs</returns>
    Task<ServiceResult<List<FileAttachmentResponseDto>>> UploadMultipleAsync(string referenceTable, int referenceId, FileAttachmentMultipleUploadViewModel model);

    /// <summary>
    /// Updates file attachment metadata (description, isPrimary)
    /// </summary>
    /// <param name="id">File attachment ID</param>
    /// <param name="model">Update data</param>
    /// <returns>Updated file attachment response DTO</returns>
    Task<ServiceResult<FileAttachmentResponseDto>> UpdateAsync(int id, FileAttachmentUpdateViewModel model);

    /// <summary>
    /// Deletes a file attachment (soft delete)
    /// </summary>
    /// <param name="id">File attachment ID</param>
    /// <returns>Success or failure result</returns>
    Task<ServiceResult> DeleteAsync(int id);

    /// <summary>
    /// Downloads a file attachment
    /// </summary>
    /// <param name="id">File attachment ID</param>
    /// <returns>File bytes</returns>
    Task<ServiceResult<byte[]>> DownloadAsync(int id);

    /// <summary>
    /// Gets the download URL for a file attachment
    /// </summary>
    /// <param name="id">File attachment ID</param>
    /// <returns>File URL string</returns>
    Task<ServiceResult<string>> GetFileUrlAsync(int id);

    /// <summary>
    /// Checks if a file attachment is an image (for preview)
    /// </summary>
    /// <param name="id">File attachment ID</param>
    /// <returns>True if file is an image</returns>
    Task<ServiceResult<bool>> IsImageFileAsync(int id);
}