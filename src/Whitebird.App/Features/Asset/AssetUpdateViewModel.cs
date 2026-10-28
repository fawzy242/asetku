using System.ComponentModel.DataAnnotations;
using Whitebird.Domain.Features.Asset;

namespace Whitebird.App.Features.Asset;

/// <summary>
/// DTO khusus untuk Update Asset dengan file upload (base64)
/// </summary>
public class AssetUpdateFormDto
{
    [Required(ErrorMessage = "AssetCode is required")]
    [StringLength(50, ErrorMessage = "AssetCode cannot exceed 50 characters")]
    public string AssetCode { get; set; } = default!;

    [Required(ErrorMessage = "AssetName is required")]
    [StringLength(100, ErrorMessage = "AssetName cannot exceed 100 characters")]
    public string AssetName { get; set; } = default!;

    [Required(ErrorMessage = "CategoryId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than 0")]
    public int CategoryId { get; set; }

    [StringLength(50, ErrorMessage = "Brand cannot exceed 50 characters")]
    public string? Brand { get; set; }

    [StringLength(50, ErrorMessage = "Model cannot exceed 50 characters")]
    public string? Model { get; set; }

    [StringLength(50, ErrorMessage = "SerialNumber cannot exceed 50 characters")]
    public string? SerialNumber { get; set; }

    [StringLength(50, ErrorMessage = "IMEI cannot exceed 50 characters")]
    public string? Imei { get; set; }

    [StringLength(50, ErrorMessage = "MacAddress cannot exceed 50 characters")]
    public string? MacAddress { get; set; }

    public DateTime? PurchaseDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "PurchasePrice must be positive")]
    public decimal? PurchasePrice { get; set; }

    [StringLength(50, ErrorMessage = "InvoiceNumber cannot exceed 50 characters")]
    public string? InvoiceNumber { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "SupplierId must be greater than 0")]
    public int? SupplierId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "WarrantyPeriod must be positive")]
    public int? WarrantyPeriod { get; set; }

    public int? AssetConditionPurchase { get; set; }

    [StringLength(50, ErrorMessage = "Hostname cannot exceed 50 characters")]
    public string? Hostname { get; set; }

    [StringLength(50, ErrorMessage = "IpAddress cannot exceed 50 characters")]
    public string? IpAddress { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
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