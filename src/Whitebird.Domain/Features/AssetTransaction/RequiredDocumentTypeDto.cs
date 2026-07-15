namespace Whitebird.Domain.Features.AssetTransaction;

/// <summary>
/// DTO for required document type
/// </summary>
public class RequiredDocumentTypeDto
{
    public int DocumentType { get; set; }
    public string? DocumentTypeName { get; set; }
    public bool IsRequired { get; set; }
    public bool IsUploaded { get; set; }
    public int? AttachmentId { get; set; }
}