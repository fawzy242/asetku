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
/// Attachment URLs produced by this service (see FileUrl / PreviewUrl on
/// <see cref="FileAttachmentResponseDto"/>) always resolve to the owning
/// menu's endpoint — never to a FileAttachment-owned route.
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
    /// (e.g. "Asset", "Employee", "AssetTransaction") so the response
    /// carries a URL that points at the owning menu's download endpoint.
    /// </summary>
    Task<ServiceResult<FileAttachmentResponseDto>> UploadAsync(
        string referenceTable, int referenceId, FileAttachmentUploadDto model);

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
    /// Uploads multiple file attachments, each associated with the given reference.
    /// </summary>
    Task<ServiceResult<List<FileAttachmentResponseDto>>> UploadMultipleAsync(
        string referenceTable, int referenceId, FileAttachmentMultipleUploadViewModel model);

    /// <summary>
    /// Updates file attachment metadata (description, isPrimary).
    /// Because this method has no reference context, the response's FileUrl /
    /// PreviewUrl are null; callers that need URLs should resolve them via the
    /// owning menu's download endpoint.
    /// </summary>
    Task<ServiceResult<FileAttachmentResponseDto>> UpdateAsync(int id, FileAttachmentUpdateViewModel model);

    /// <summary>
    /// Soft-deletes a file attachment (IsActive = 0). Physical file is retained.
    /// </summary>
    Task<ServiceResult> DeleteAsync(int id);

    /// <summary>
    /// Reads the raw bytes and metadata of a stored attachment so callers
    /// (menu-level services) can stream it back through their own endpoint.
    /// </summary>
    Task<ServiceResult<FileContentDto>> DownloadAsync(int id);

    /// <summary>
    /// Returns a download URL for a file attachment.
    ///
    /// NOTE: URLs are context-dependent in this architecture — the correct
    /// download route is owned by the menu the file belongs to. This
    /// context-blind overload therefore cannot produce a valid URL and
    /// returns NotFound. Callers should obtain URLs via
    /// <see cref="FileAttachmentResponseDto.FileUrl"/> (populated by upload)
    /// or by constructing the owning menu's endpoint themselves.
    /// </summary>
    Task<ServiceResult<string>> GetFileUrlAsync(int id);

    /// <summary>
    /// Checks whether the underlying file has an image extension (for preview).
    /// </summary>
    Task<ServiceResult<bool>> IsImageFileAsync(int id);
}