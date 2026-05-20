using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.FileAttachment;

public class FileAttachmentEntity : AuditableEntity
{
    public int FileAttachmentId { get; set; }
    public string ReferenceTable { get; set; } = default!;
    public int ReferenceId { get; set; }
    public string? FileCategory { get; set; }
    public string FileName { get; set; } = default!;
    public string OriginalFileName { get; set; } = default!;
    public string? FileExtension { get; set; }
    public string? FileMimeType { get; set; }
    public string FilePath { get; set; } = default!;
    public long? FileSize { get; set; }
    public string? FileHash { get; set; }
    public string? Description { get; set; }
    public int VersionNumber { get; set; } = 1;
    public bool IsPrimary { get; set; }
}