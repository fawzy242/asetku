namespace Whitebird.Domain.Features.Asset;

/// <summary>
/// View model for asset detail (response from GetById)
/// </summary>
public class AssetDetailView
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Imei { get; set; }
    public string? MacAddress { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public string? InvoiceNumber { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int? WarrantyPeriod { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    public int? AssetCondition { get; set; }
    public string? AssetConditionName { get; set; }
    public int? AssetConditionPurchase { get; set; }
    public string? AssetConditionPurchaseName { get; set; }
    public decimal? ResidualValue { get; set; }
    public int? UsefulLife { get; set; }
    public DateTime? DepreciationStartDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public int? OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public string? Hostname { get; set; }
    public string? IpAddress { get; set; }
    public bool? OperasionalOffice { get; set; }
    public string? CurrentStatus { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "System";
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
}