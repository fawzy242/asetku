namespace Whitebird.Domain.Features.FileAttachment;

/// <summary>
/// Standardized file resource DTO shared across all menus
/// (Asset, Employee, AssetTransaction, User, and their sub-detail endpoints).
///
/// Provides a stable, environment-independent contract for the frontend:
///   * <see cref="DownloadUrl"/> is a RELATIVE URL
///     (e.g. "/api/Employee/1/attachments/62/download").
///     The frontend prepends its configured backend base URL.
///   * <see cref="ContentType"/> is the server-authoritative MIME type
///     (from FileAttachment.FileMimeType).
///   * <see cref="FileUrl"/> is a temporary backward-compatibility alias for
///     <see cref="DownloadUrl"/>. It will be removed after the frontend
///     migration completes.
///
/// Menu-specific derived types (e.g. EmployeeAttachmentViewDto) extend this
/// class by adding their join-table ID (EmployeeAttachmentId, AssetAttachmentId,
/// AssetTransactionAttachmentId, ...). All other fields are inherited so the
/// JSON shape is consistent across menus.
/// </summary>
public class FileResourceDto
{
    public int FileAttachmentId { get; set; }

    /// <summary>Stored (GUID-based) file name — server-generated.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Original user-provided file name (display name).</summary>
    public string OriginalFileName { get; set; } = string.Empty;

    public string? FileCategory { get; set; }

    /// <summary>MIME type from FileAttachment.FileMimeType.</summary>
    public string? ContentType { get; set; }

    /// <summary>File size in bytes.</summary>
    public long? FileSize { get; set; }

    public bool IsPrimary { get; set; }

    /// <summary>Menu-specific ordering hint (0 when not applicable).</summary>
    public int Order { get; set; }

    /// <summary>
    /// Relative URL to the menu's download endpoint.
    /// Populated by <c>FileResourceUrlResolver</c> in the owning menu's service.
    /// </summary>
    public string? DownloadUrl { get; set; }

    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Backward-compatibility alias for <see cref="DownloadUrl"/>.
    /// Getter and setter are wired to <see cref="DownloadUrl"/>, so both
    /// properties always carry the same value. Serializers will emit both
    /// JSON fields ("fileUrl" and "downloadUrl") with identical content.
    /// Will be removed after the frontend migration completes.
    /// </summary>
    [Obsolete("Use DownloadUrl. Kept temporarily for frontend backward compatibility.")]
    public string? FileUrl
    {
        get => DownloadUrl;
        set => DownloadUrl = value;
    }
}