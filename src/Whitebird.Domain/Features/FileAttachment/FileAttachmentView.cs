using System.ComponentModel.DataAnnotations;

namespace Whitebird.Domain.Features.FileAttachment;

public class FileAttachmentListViewModel
{
    public int FileAttachmentId { get; set; }
    public string ReferenceTable { get; set; } = default!;
    public int ReferenceId { get; set; }
    public string? FileCategory { get; set; }
    public string FileName { get; set; } = default!;
    public string OriginalFileName { get; set; } = default!;
    public string? FileExtension { get; set; }
    public string? FileMimeType { get; set; }
    public long? FileSize { get; set; }
    public string? Description { get; set; }
    public int VersionNumber { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
}

public class FileAttachmentDetailViewModel
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
    public int VersionNumber { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}