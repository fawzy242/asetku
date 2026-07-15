namespace Whitebird.Domain.Features.AssetTransaction;

/// <summary>
/// DTO for approve document type
/// </summary>
public class ApproveDocumentTypeDto
{
    public int DocumentType { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
}