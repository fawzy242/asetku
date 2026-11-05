namespace Whitebird.App.Features.FileAttachment;

/// <summary>
/// Identifies the owning menu for a file resource.
/// Photo owners vs. attachment owners are distinct because they map to
/// different endpoints (inline photo vs. attachment download).
/// </summary>
public enum FileOwner
{
    /// <summary>Asset attachment (download endpoint).</summary>
    Asset,

    /// <summary>Asset profile photo (inline photo endpoint).</summary>
    AssetPhotoProfile,

    /// <summary>Employee attachment (download endpoint).</summary>
    Employee,

    /// <summary>Employee profile photo (inline photo endpoint).</summary>
    EmployeePhotoProfile,

    /// <summary>Asset transaction attachment (download endpoint).</summary>
    AssetTransaction,

    /// <summary>Asset transaction approval-document attachment (download endpoint).</summary>
    AssetTransactionApprove,

    /// <summary>User profile photo (inline photo endpoint, session-scoped).</summary>
    UserPhotoProfile
}

/// <summary>
/// Central resolver for file resource URLs.
///
/// Produces RELATIVE URLs only (leading "/api/..."). Never uses
/// IHttpContextAccessor, never references scheme/host/port. Safe behind
/// reverse proxies and multi-host deployments: the frontend prepends its
/// configured backend base URL.
///
/// This is the single source of truth for URL formatting; every menu-specific
/// service or repository that emits a PhotoProfileUrl, DownloadUrl, or FileUrl
/// must call this resolver instead of building URLs inline or in SQL.
/// </summary>
public static class FileResourceUrlResolver
{
    /// <summary>
    /// Returns the relative photo URL for the given owner, or null when the
    /// owner has no photo endpoint (attachment owners return null).
    /// </summary>
    public static string? ResolvePhotoUrl(FileOwner owner, int ownerId)
    {
        if (ownerId <= 0) return null;

        return owner switch
        {
            FileOwner.AssetPhotoProfile => $"/api/Asset/{ownerId}/photo",
            FileOwner.EmployeePhotoProfile => $"/api/Employee/{ownerId}/photo",
            FileOwner.UserPhotoProfile => "/api/User/profile-photo",
            _ => null
        };
    }

    /// <summary>
    /// Returns the relative download URL for the given owner, or null when the
    /// owner has no download endpoint (photo owners return null).
    /// </summary>
    public static string? ResolveDownloadUrl(FileOwner owner, int ownerId, int fileAttachmentId)
    {
        if (ownerId <= 0 || fileAttachmentId <= 0) return null;

        return owner switch
        {
            FileOwner.Asset =>
                $"/api/Asset/{ownerId}/attachments/{fileAttachmentId}/download",
            FileOwner.Employee =>
                $"/api/Employee/{ownerId}/attachments/{fileAttachmentId}/download",
            FileOwner.AssetTransaction =>
                $"/api/AssetTransaction/{ownerId}/attachments/{fileAttachmentId}/download",
            FileOwner.AssetTransactionApprove =>
                $"/api/AssetTransaction/{ownerId}/approve-attachments/{fileAttachmentId}/download",
            _ => null
        };
    }
}