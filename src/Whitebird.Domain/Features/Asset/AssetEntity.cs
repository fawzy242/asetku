using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.Asset;

public class AssetEntity : AuditableEntity
{
    public int AssetId { get; set; }
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
    public DateTime? WarrantyExpiryDate { get; set; }
    public int? AssetCondition { get; set; }
    public int? AssetConditionPurchase { get; set; }
    public decimal? ResidualValue { get; set; }
    public int? UsefulLife { get; set; }
    public DateTime? DepreciationStartDate { get; set; }
    public string? Notes { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public int? OfficeId { get; set; }
    public string? Hostname { get; set; }
    public string? IpAddress { get; set; }
    public bool? OperasionalOffice { get; set; }
    
    // NOTE: Semua NotMapped properties telah dihapus.
    // Untuk query dengan JOIN, gunakan AssetListView, AssetDetailView, atau AssetTrackingView.
}