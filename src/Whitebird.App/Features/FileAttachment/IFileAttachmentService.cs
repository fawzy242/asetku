using Whitebird.App.Features.Common;
using Whitebird.Domain.Features.FileAttachment;

namespace Whitebird.App.Features.FileAttachment;

/// <summary>
/// Internal service for file attachment storage, metadata, and streaming.
///
/// This service has NO controller. Each menu that owns attachments
/// (Asset, Employee, AssetTransaction, User) exposes its own upload /
/// download / delete endpoints and delegates to this service internally.
///
/// URL contract:
///   * All URLs produced by this service (via <c>FileResourceUrlResolver</c>)
///     are RELATIVE (e.g. "/api/Employee/1/attachments/62/download").
///   * The frontend prepends its configured backend base URL.
///   * This service never references scheme, host, or port.
///
/// Menu-specific authorization and ownership checks remain in each menu's
/// service layer. This service does NOT enforce ownership — callers must do so
/// before calling <see cref="DownloadAsync"/>.
/// </summary>
public interface IFileAttachmentService
{
    /// <summary>
    /// Gets a file attachment's metadata by ID.
    /// </summary>
    Task<ServiceResult<FileAttachmentDetailViewModel>> GetByIdAsync(int id);

    /// <summary>
    /// Uploads a single file attachment (from IFormFile).
    /// <paramref name="referenceTable"/> identifies the owning menu
    /// (e.g. "Asset", "Employee", "AssetTransaction") so the response carries
    /// a relative URL that points at the owning menu's download endpoint.
    /// </summary>
    Task<ServiceResult<FileAttachmentResponseDto>> UploadAsync(
        string referenceTable, int referenceId, FileAttachmentUploadDto model);

    /// <summary>
    /// Uploads a single file attachment from raw bytes.
    /// Used by services that receive base64 payloads (Asset, Employee,
    /// AssetTransaction, User profile photo).
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
    /// Uploads multiple file attachments, each associated with the given reference.
    /// </summary>
    Task<ServiceResult<List<FileAttachmentResponseDto>>> UploadMultipleAsync(
        string referenceTable, int referenceId, FileAttachmentMultipleUploadViewModel model);

    /// <summary>
    /// Updates file attachment metadata (description, isPrimary).
    /// Because this method has no reference context, the response's
    /// <c>DownloadUrl</c> / <c>PreviewUrl</c> are null; callers that need URLs
    /// should resolve them via <c>FileResourceUrlResolver</c>.
    /// </summary>
    Task<ServiceResult<FileAttachmentResponseDto>> UpdateAsync(int id, FileAttachmentUpdateViewModel model);

    /// <summary>
    /// Soft-deletes a file attachment's metadata (IsActive = 0) and removes
    /// the physical file from storage. Physical-delete failures are logged as
    /// warnings and do not roll back the metadata change. Idempotent when the
    /// file is already absent from disk.
    /// </summary>
    Task<ServiceResult> DeleteAsync(int id);

    /// <summary>
    /// Reads the raw bytes and metadata of a stored attachment so callers
    /// (menu-level services) can stream it back through their own endpoint.
    /// Callers must enforce entity ownership before calling this method.
    /// </summary>
    Task<ServiceResult<FileContentDto>> DownloadAsync(int id);

    /// <summary>
    /// Checks whether the underlying file has an image extension (for preview).
    /// </summary>
    Task<ServiceResult<bool>> IsImageFileAsync(int id);
}