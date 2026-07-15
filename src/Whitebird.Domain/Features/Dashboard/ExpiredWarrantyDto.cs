namespace Whitebird.Domain.Features.Dashboard;

/// <summary>
/// DTO for expired warranty assets
/// </summary>
public class ExpiredWarrantyDto
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    public string? SupplierName { get; set; }
    public string? CurrentStatus { get; set; }
}