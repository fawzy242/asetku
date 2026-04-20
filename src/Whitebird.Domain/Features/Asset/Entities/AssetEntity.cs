using Whitebird.Domain.Features.Common.Entities;
using Whitebird.Domain.Features.Common.Attributes;

namespace Whitebird.Domain.Features.Asset.Entities;

public class AssetEntity : AuditableEntity
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public int CategoryId { get; set; }
    public string? SubCategory { get; set; }
    public string? AssetType { get; set; }
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
    public DateTime? WarrantyExpiryDate { get; set; }
    public string? Condition { get; set; }
    public string Status { get; set; } = "Available";
    public string? Location { get; set; }
    public int? CurrentHolderId { get; set; }
    public int? ResponsiblePartyId { get; set; }
    public decimal? ResidualValue { get; set; }
    public int? UsefulLife { get; set; }
    public DateTime? DepreciationStartDate { get; set; }
    public string? Notes { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }

    // Navigation properties - marked as NotMapped
    [NotMapped]
    public string? CategoryName { get; set; }

    [NotMapped]
    public string? CurrentHolderName { get; set; }

    [NotMapped]
    public string? SupplierName { get; set; }

    [NotMapped]
    public string? ResponsiblePartyName { get; set; }
}