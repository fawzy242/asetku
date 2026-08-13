using Whitebird.Domain.Features.Asset;

namespace Whitebird.App.Features.Asset;

/// <summary>
/// DTO khusus untuk Update Asset dengan file upload (base64)
/// </summary>
public class AssetUpdateFormDto
{
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public int CategoryId { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Imei { get; set; }
    public string? MacAddress { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public string? InvoiceNumber { get; set; }
    public int? SupplierId { get; set; }
    public int? WarrantyPeriod { get; set; }
    public int? AssetConditionPurchase { get; set; }
    public string? Hostname { get; set; }
    public string? IpAddress { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public SoftwareInstallationDto? SoftwareInstallation { get; set; }

    // File upload sebagai base64 (bukan IFormFile)
    public string? PhotoFileBase64 { get; set; }
    public string? PhotoFileName { get; set; }
    public List<string>? AttachmentFilesBase64 { get; set; }
    public List<string>? AttachmentFileNames { get; set; }
    public List<int>? AttachmentIdsToKeep { get; set; }
    public bool ReplaceAttachments { get; set; } = false;
}