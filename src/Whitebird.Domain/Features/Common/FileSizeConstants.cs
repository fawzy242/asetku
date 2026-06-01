namespace Whitebird.Domain.Features.Common;

/// <summary>
/// Centralized constants for file size limits across the application
/// </summary>
public static class FileSizeConstants
{
    /// <summary>
    /// Maximum file size for asset attachments in megabytes
    /// </summary>
    public const int MaxAssetAttachmentMB = 10;
    
    /// <summary>
    /// Maximum file size for profile photos in megabytes
    /// </summary>
    public const int MaxProfilePhotoMB = 5;
    
    /// <summary>
    /// Maximum file size for asset attachments in bytes
    /// </summary>
    public const long MaxAssetAttachmentBytes = MaxAssetAttachmentMB * 1024 * 1024;
    
    /// <summary>
    /// Maximum file size for profile photos in bytes
    /// </summary>
    public const long MaxProfilePhotoBytes = MaxProfilePhotoMB * 1024 * 1024;
    
    /// <summary>
    /// Allowed file extensions for asset attachments
    /// </summary>
    public static readonly string[] AllowedAssetExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" };
    
    /// <summary>
    /// Allowed file extensions for profile photos
    /// </summary>
    public static readonly string[] AllowedProfilePhotoExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    
    /// <summary>
    /// Checks if a file extension is allowed for asset attachments
    /// </summary>
    public static bool IsAllowedAssetExtension(string extension)
    {
        return AllowedAssetExtensions.Contains(extension.ToLowerInvariant());
    }
    
    /// <summary>
    /// Checks if a file extension is allowed for profile photos
    /// </summary>
    public static bool IsAllowedProfilePhotoExtension(string extension)
    {
        return AllowedProfilePhotoExtensions.Contains(extension.ToLowerInvariant());
    }
}