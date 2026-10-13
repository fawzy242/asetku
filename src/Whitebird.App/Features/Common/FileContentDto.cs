namespace Whitebird.App.Features.Common;

/// <summary>
/// Result wrapper for file content delivery.
/// Carries the correct Content-Type so callers (controllers) can return
/// the file with the proper MIME type instead of a hardcoded value.
/// </summary>
public class FileContentDto
{
    public byte[] Bytes { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
    public string FileName { get; set; } = string.Empty;
}