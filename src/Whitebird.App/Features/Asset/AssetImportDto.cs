namespace Whitebird.App.Features.Asset;

public class AssetImportDto
{
    public string AssetCode { get; set; } = default!;
    public string AssetName { get; set; } = default!;
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Imei { get; set; }
    public string? MacAddress { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? PurchasePrice { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? SupplierName { get; set; }
    public int? SupplierId { get; set; }
    public int? WarrantyPeriod { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    public string? AssetCondition { get; set; }
    public string? AssetConditionPurchase { get; set; }
    public string? OfficeName { get; set; }
    public int? OfficeId { get; set; }
    public string? Hostname { get; set; }
    public string? IpAddress { get; set; }
    public bool? OperasionalOffice { get; set; }
    public decimal? ResidualValue { get; set; }
    public int? UsefulLife { get; set; }
    public DateTime? DepreciationStartDate { get; set; }
    public string? Notes { get; set; }
}