using Microsoft.AspNetCore.Http;
using Whitebird.Domain.Features.Common;

namespace Whitebird.App.Features.Common;

/// <summary>
/// Centralised file-upload validation (extension + size) used by all attachment
/// and photo upload flows.
///
/// Two entry points:
///   * <see cref="Validate(IFormFile, string[], long, int)"/> — for requests bound to IFormFile
///   * <see cref="Validate(string, long, string[], long, int)"/> — for base64/byte payloads
///
/// Convenience wrappers cover the two fixed profiles in the system:
///   * <see cref="ValidateProfilePhoto(IFormFile)"/> — 5 MB, jpg/jpeg/png/gif/webp
///   * <see cref="ValidateAssetAttachment(IFormFile)"/> — 10 MB, broader set
///
/// Returns a tuple so callers can shape the response through their own
/// <c>ServiceResult&lt;T&gt;</c>. The error messages are identical to the ones
/// the previous inline implementations produced — this helper is a refactor
/// only, not a behaviour change.
/// </summary>
public static class FileUploadValidator
{
    /// <summary>
    /// Validates a profile-photo upload against the fixed profile-photo rules
    /// (5 MB, jpg/jpeg/png/gif/webp).
    /// </summary>
    public static (bool IsValid, string? Error) ValidateProfilePhoto(IFormFile? file)
    {
        return Validate(
            file,
            FileSizeConstants.AllowedProfilePhotoExtensions,
            FileSizeConstants.MaxProfilePhotoBytes,
            FileSizeConstants.MaxProfilePhotoMB);
    }

    /// <summary>
    /// Validates an asset or employee attachment against the fixed asset-attachment
    /// rules (10 MB, broader extension set).
    /// </summary>
    public static (bool IsValid, string? Error) ValidateAssetAttachment(IFormFile? file)
    {
        return Validate(
            file,
            FileSizeConstants.AllowedAssetExtensions,
            FileSizeConstants.MaxAssetAttachmentBytes,
            FileSizeConstants.MaxAssetAttachmentMB);
    }

    /// <summary>
    /// Core overload for <see cref="IFormFile"/> uploads.
    /// </summary>
    public static (bool IsValid, string? Error) Validate(
        IFormFile? file,
        string[] allowedExtensions,
        long maxSizeBytes,
        int maxSizeMb)
    {
        if (file == null || file.Length == 0)
        {
            return (false, "No file provided");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (Array.IndexOf(allowedExtensions, extension) < 0)
        {
            return (false,
                $"File type '{extension}' is not allowed. Allowed: {string.Join(", ", allowedExtensions)}");
        }

        if (file.Length > maxSizeBytes)
        {
            return (false, $"File size exceeds {maxSizeMb}MB limit");
        }

        return (true, null);
    }

    /// <summary>
    /// Core overload for byte / base64 payloads. The caller supplies the original
    /// file name (used only to determine the extension).
    /// </summary>
    public static (bool IsValid, string? Error) Validate(
        string? originalFileName,
        long lengthBytes,
        string[] allowedExtensions,
        long maxSizeBytes,
        int maxSizeMb)
    {
        if (lengthBytes <= 0)
        {
            return (false, "No file content provided");
        }

        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            return (false, "Original file name is required");
        }

        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (Array.IndexOf(allowedExtensions, extension) < 0)
        {
            return (false,
                $"File type '{extension}' is not allowed. Allowed: {string.Join(", ", allowedExtensions)}");
        }

        if (lengthBytes > maxSizeBytes)
        {
            return (false, $"File size exceeds {maxSizeMb}MB limit");
        }

        return (true, null);
    }
}