using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Asset;

public class AssetEntity : AuditableEntity
{
    public int AssetId { get; set; }
    public string? Notes { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public int CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Imei { get; set; }
    public string? Hostname { get; set; }
    public string? IpAddress { get; set; }
    public string? MacAddress { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public string? InvoiceNumber { get; set; }
    public int? WarrantyPeriod { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    public int? AssetConditionPurchase { get; set; }
}

public class AssetAttachmentEntity : AuditableEntity
{
    public int AssetAttachmentId { get; set; }
    public int AssetId { get; set; }
    public int FileAttachmentId { get; set; }
    public int Order { get; set; }
}

public class AssetPhotoProfileAttachmentEntity : AuditableEntity
{
    public int AssetPhotoProfileAttachmentId { get; set; }
    public int AssetId { get; set; }
    public int FileAttachmentId { get; set; }
}

public class SoftwareInstallationEntity : AuditableEntity
{
    public int SoftwareInstallationId { get; set; }
    public int AssetId { get; set; }
    public string? UserPassword { get; set; }
    public string? AdminOperationalPassword { get; set; }
    public int? OperatingSystem { get; set; }
    public int? MicrosoftOffice { get; set; }
    public int? VisualStudio { get; set; }
    public int? DBMS { get; set; }
    public bool ConnectToNAS { get; set; }
}